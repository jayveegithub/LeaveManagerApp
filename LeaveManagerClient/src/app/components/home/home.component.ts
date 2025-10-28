import { Component, OnInit, Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UserService, User } from '../../services/user.service';
import { LeaveService, LeaveType, LeaveRequest } from '../../services/leave.service';
import { MatIconModule } from '@angular/material/icon';
import { Observable, forkJoin, of } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatCardModule,
    MatSnackBarModule,
    MatIconModule
  ]
})
@Injectable({
  providedIn: 'root'
}) 
export class HomeComponent implements OnInit {
  leaveForm!: FormGroup;
  users: User[] = [];
  managers: User[] = [];
  leaveTypes: LeaveType[] = [];
  holidays: string[] = [];
  minStartDate = new Date(); // today
  loading = false;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private leaveService: LeaveService,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.buildForm();
    // load users, leave types and holidays
    forkJoin({
      users: this.userService.getUsers().pipe(
        catchError(() => of([]))
      ),
      leaveTypes: this.leaveService.getLeaveTypes().pipe(
        catchError(() => of([]))
      ),
      holidays: this.leaveService.getHolidays().pipe(
        catchError(() => of([]))
      ),
      managers: this.userService.getManagers().pipe(
        catchError(() => of([]))
      ), 
    }).subscribe(res => {
      this.users = this.userService.mapFullName(res.users as User[]);
      this.managers = this.userService.mapFullName(res.managers as User[]);
      this.leaveTypes = (res.leaveTypes as LeaveType[]).filter(t => t.isActive);
      console.log('leaveTypes:', res.leaveTypes);
      this.holidays = res.holidays as string[];
    });

    // Recalculate working days when dates change
    this.leaveForm.get('startDate')?.valueChanges.subscribe(() => this.recalculateDays());
    this.leaveForm.get('endDate')?.valueChanges.subscribe(() => this.recalculateDays());
  }

  private buildForm() {
    this.leaveForm = this.fb.group({
      applicant: ['', Validators.required],
      manager: ['', Validators.required],
      leaveTypeId: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      returnDate: ['', Validators.required],
      numberOfDays: [0, [Validators.required, Validators.min(1)]],  // todo: recompute when dates change
      generalComments: ['', [Validators.maxLength(500)]]
    }, { validators: [this.applicantManagerDifferent(), this.dateSequenceValidator()] });
  }

  private applicantManagerDifferent() {
    return (group: AbstractControl) => {
      const a = group.get('applicant')?.value;
      const m = group.get('manager')?.value;
      if (a && m && a === m) {
        group.get('manager')?.setErrors({ sameAsApplicant: true });
      } else {
        const ctrl = group.get('manager');
        if (ctrl?.hasError('sameAsApplicant')) ctrl.updateValueAndValidity({ onlySelf: true, emitEvent: false });
      }
      return null;
    };
  }

  private dateSequenceValidator() {
    return (group: AbstractControl) => {
      const start: Date = group.get('startDate')?.value;
      const end: Date = group.get('endDate')?.value;
      const ret: Date = group.get('returnDate')?.value;

      // Clear previous errors
      ['startDate', 'endDate', 'returnDate'].forEach(k => {
        const c = group.get(k);
        if (c && c.errors) {
          const { endBeforeStart, returnBeforeEnd, pastDate, ...rest } = c.errors as any;
          if (!Object.keys(rest).length) c.setErrors(null);
          else c.setErrors(rest);
        }
      });

      const today = new Date(); today.setHours(0,0,0,0);

      if (start) {
        const s = new Date(start); s.setHours(0,0,0,0);
        if (s < today) group.get('startDate')?.setErrors({ ...group.get('startDate')?.errors, pastDate: true });
      }
      if (start && end) {
        const s = new Date(start).setHours(0,0,0,0);
        const e = new Date(end).setHours(0,0,0,0);
        if (e <= s) group.get('endDate')?.setErrors({ ...group.get('endDate')?.errors, endBeforeStart: true });
      }
      if (end && ret) {
        const e = new Date(end).setHours(0,0,0,0);
        const r = new Date(ret).setHours(0,0,0,0);
        if (r <= e) group.get('returnDate')?.setErrors({ ...group.get('returnDate')?.errors, returnBeforeEnd: true });
      }
      return null;
    };
  }

  private recalculateDays() {
    const s = this.leaveForm.get('startDate')?.value;
    const e = this.leaveForm.get('endDate')?.value;
    if (!s || !e) return;
    const start = new Date(s);
    const end = new Date(e);
    if (end < start) return;
    const count = this.leaveService.workingDaysBetween(start, end, this.holidays);
    this.leaveForm.patchValue({ numberOfDays: count }, { emitEvent: false });
  }

  // Client-side check for weekends/holidays for the three dates
  private checkWorkingDaysForDates(start: Date, end: Date, ret: Date): string | null {
    if (this.leaveService.isWeekendOrHoliday(start, this.holidays)) return 'Start date falls on a weekend or holiday';
    if (this.leaveService.isWeekendOrHoliday(end, this.holidays)) return 'End date falls on a weekend or holiday';
    if (this.leaveService.isWeekendOrHoliday(ret, this.holidays)) return 'Return date falls on a weekend or holiday';
    return null;
  }

  onSubmit(): void {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }
    this.loading = true;

    const raw = this.leaveForm.value;
    const candidate: LeaveRequest = {
      applicantId: raw.applicant,
      managerId: raw.manager,
      leaveTypeId: +raw.leaveTypeId,
      startDate: this.leaveService.toIsoDate(raw.startDate),
      endDate: this.leaveService.toIsoDate(raw.endDate),
      returnDate: this.leaveService.toIsoDate(raw.returnDate),
      numberOfDays: +raw.numberOfDays,
      generalComments: raw.generalComments
    };

    // Check leave type constraints
    const selectedType = this.leaveTypes.find(t => t.id === candidate.leaveTypeId);
    if (!selectedType) {
      this.snackBar.open('Selected leave type is not valid/active', 'Close', { duration: 5000 });
      this.loading = false;
      return;
    }
    if (candidate.numberOfDays > selectedType.maxDurationDays) {
      this.snackBar.open(`Leave exceeds maximum duration for ${selectedType.name}`, 'Close', { duration: 5000 });
      this.loading = false;
      return;
    }

    // Check min advance days (working days)
    const today = new Date(); today.setHours(0,0,0,0);
    const startDate = new Date(candidate.startDate);
    const daysInAdvance = this.leaveService.workingDaysBetween(today, startDate, this.holidays) - 1; // exclude today
    if (daysInAdvance < selectedType.minAdvanceDays) {
      this.snackBar.open(`Leave must be requested at least ${selectedType.minAdvanceDays} working days in advance`, 'Close', { duration: 6000 });
      this.loading = false;
      return;
    }

    // Weekend/holiday checks
    const weekendHolidayError = this.checkWorkingDaysForDates(new Date(candidate.startDate), new Date(candidate.endDate), new Date(candidate.returnDate));
    if (weekendHolidayError) {
      this.snackBar.open(weekendHolidayError, 'Close', { duration: 5000 });
      this.loading = false;
      return;
    }

    // Fetch existing leaves for applicant to run duplicate/overlap checks then submit
    this.leaveService.getLeavesForApplicant(candidate.applicantId).pipe(
      switchMap(existing => {
        if (this.leaveService.isDuplicate(candidate, existing)) {
          this.snackBar.open('Duplicate leave request detected', 'Close', { duration: 5000 });
          this.loading = false;
          return of(null);
        }
        const overlap = existing.some(e => this.leaveService.isOverlapping(candidate.startDate, candidate.endDate, e.startDate, e.endDate));
        if (overlap) {
          this.snackBar.open('This request overlaps with an existing leave for the applicant', 'Close', { duration: 5000 });
          this.loading = false;
          return of(null);
        }
        // All checks passed - submit
        return this.leaveService.createLeave(candidate).pipe(
          catchError(err => {
            this.snackBar.open('Server error creating leave request', 'Close', { duration: 5000 });
            this.loading = false;
            return of(null);
          })
        );
      })
    ).subscribe(result => {
      this.loading = false;
      if (result) {
        this.snackBar.open('Leave request submitted', 'Close', { duration: 4000 });
        this.leaveForm.reset();
      }
    });
  }

  // Helpers for template
  get f(): LeaveControls {
    return this.leaveForm.controls as unknown as LeaveControls;
  }
}

type LeaveControls = {
  applicant: AbstractControl;
  manager: AbstractControl;
  leaveTypeId: AbstractControl;
  startDate: AbstractControl;
  endDate: AbstractControl;
  returnDate: AbstractControl;
  numberOfDays: AbstractControl;
  generalComments: AbstractControl;
};

