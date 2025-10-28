import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ConfigService } from './config.service';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresIn: number;
  userId: string;
  fullName: string;
}

@Injectable({
  providedIn: 'any'
})
export class AuthService {
  private authStateSubject = new BehaviorSubject<boolean>(false);
  public authState$ = this.authStateSubject.asObservable();

  constructor(
    private http: HttpClient,
    private configSvc: ConfigService) {
    this.checkInitialAuth();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.configSvc.getApiUrl()}/api/Auth/login`, credentials)
      .pipe(
        tap(response => {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify({
            userId: response.userId,
            fullName: response.fullName
          }));
          this.authStateSubject.next(true);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authStateSubject.next(false);
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem('token');
    if (!token) return false;
    
    try {
      // Basic JWT expiration check
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expired = Date.now() >= payload.exp * 1000;
      return !expired;
    } catch {
      return false;
    }
  }

  private checkInitialAuth(): void {
    const token = localStorage.getItem('token');
    this.authStateSubject.next(!!token);
  }
}
