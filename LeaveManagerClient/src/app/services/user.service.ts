import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  email?: string;
}

@Injectable({ providedIn: 'any' })
export class UserService {
  constructor(
    private http: HttpClient,
    private configSvc: ConfigService
  ) {}
  
  // Adjust endpoint as required
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.configSvc.getApiUrl()}/api/users`);
  }

  getManagers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.configSvc.getApiUrl()}/api/users/managers`);
  } 

  // Helper to map fullName on the client if backend doesn't provide it
  mapFullName(users: User[]): User[] {
    return users.map(u => ({ ...u, fullName: u.fullName ?? `${u.firstName} ${u.lastName}` }));
  }
}