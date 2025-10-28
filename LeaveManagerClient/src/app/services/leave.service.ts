import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, map } from 'rxjs';
import { ConfigService } from './config.service';

export interface LeaveType {
  id: number;
  name: string;
  isActive: boolean;
  maxDurationDays: number; // maximum allowed duration
  minAdvanceDays: number;  // minimum working days in advance
}

export interface LeaveRequest {
  id?: string;
  applicantId: string;
  managerId: string;
  leaveTypeId: number;
  startDate: string;   // ISO yyyy-mm-dd
  endDate: string;     // ISO
  returnDate: string;  // ISO
  numberOfDays: number;
  generalComments?: string;
  createdAt?: string;
}

@Injectable({ providedIn: 'any' })
export class LeaveService {
  // Replace endpoints with real ones
  constructor(private http: HttpClient, 
    private configSvc: ConfigService) {}

  getLeaveTypes(): Observable<LeaveType[]> {
    return this.http.get<LeaveType[]>(`${this.configSvc.getApiUrl()}/api/LeaveRequests/leaveTypes`);
  }

  getLeavesForApplicant(applicantId: string): Observable<LeaveRequest[]> {
    return this.http.get<LeaveRequest[]>(`${this.configSvc.getApiUrl()}/api/LeaveRequests/applicant/${applicantId}`);
  }

  createLeave(leave: LeaveRequest): Observable<LeaveRequest> {
    return this.http.post<LeaveRequest>(`${this.configSvc.getApiUrl()}/api/LeaveRequests`, leave);
  }

  // Simple local checks; server should enforce these too.
  isOverlapping(aStart: string, aEnd: string, bStart: string, bEnd: string): boolean {
    const aS = new Date(aStart).setHours(0,0,0,0);
    const aE = new Date(aEnd).setHours(0,0,0,0);
    const bS = new Date(bStart).setHours(0,0,0,0);
    const bE = new Date(bEnd).setHours(0,0,0,0);
    return !(aE < bS || aS > bE);
  }

  isDuplicate(candidate: LeaveRequest, existing: LeaveRequest[]): boolean {
    return existing.some(e =>
      e.applicantId === candidate.applicantId &&
      e.managerId === candidate.managerId &&
      e.leaveTypeId === candidate.leaveTypeId &&
      e.startDate === candidate.startDate &&
      e.endDate === candidate.endDate &&
      e.returnDate === candidate.returnDate &&
      (e.generalComments ?? '') === (candidate.generalComments ?? '') &&
      e.numberOfDays === candidate.numberOfDays
    );
  }

  // Example static holidays - replace with API or config as needed
  getHolidays(year?: number): Observable<string[]> {
    const currentYear = year ?? new Date().getFullYear();
    const holidays = [
      `${currentYear}-01-01`,
      `${currentYear}-12-25`
    ];
    return of(holidays);
  }

  // Return true if date is weekend or in holidays list
  isWeekendOrHoliday(date: Date, holidays: string[]): boolean {
    const day = date.getDay();
    if (day === 0 || day === 6) return true;
    const iso = this.toIsoDate(date);
    return holidays.includes(iso);
  }

  toIsoDate(input: Date | string): string {
    const d = typeof input === 'string' ? new Date(input) : input;
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  // Count working days between inclusive, excluding weekends and holidays
  workingDaysBetween(start: Date, end: Date, holidays: string[] = []): number {
    const s = new Date(start); s.setHours(0,0,0,0);
    const e = new Date(end); e.setHours(0,0,0,0);
    if (e < s) return 0;
    let count = 0;
    for (let d = new Date(s); d <= e; d.setDate(d.getDate() + 1)) {
      if (!this.isWeekendOrHoliday(new Date(d), holidays)) count++;
    }
    return count;
  }
}