import { Component, Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule, MatCardTitle, MatCardHeader, MatCard, MatCardContent } from '@angular/material/card';
import { MatIcon } from "@angular/material/icon";
import { MatError, MatFormField, MatLabel, MatInputModule } from "@angular/material/input";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [MatError, MatProgressSpinner, MatFormField, MatLabel, MatCardTitle, MatCardHeader, MatCard, MatCardContent, MatInputModule,
    ReactiveFormsModule, CommonModule, HttpClientModule]
})

@Injectable({
  providedIn: 'root'
})  
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open(
            error.error?.error || 'Login failed. Please try again.',
            'Close',
            { duration: 5000 }
          );
        }
      });
    }
  }
}
