import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, AuthResponse, LoginModel, RegisterModel } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'https://localhost:7220/api/auth';

  constructor(private http: HttpClient) {}

  login(credentials: LoginModel): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, credentials);
  }

  register(data: RegisterModel): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, data);
  }

  resetPassword(email: string, newPassword: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/reset-password`, { email, newPassword });
  }

  saveSession(data: AuthResponse): void {
    localStorage.setItem('JWT-token', data.token);
    localStorage.setItem('user-role', data.role);
    localStorage.setItem('user-name', data.fullName);
    localStorage.setItem('user-email', data.email);
  }

  logout(): void {
    localStorage.removeItem('JWT-token');
    localStorage.removeItem('user-role');
    localStorage.removeItem('user-name');
    localStorage.removeItem('user-email');
  }

  getToken(): string | null {
    return localStorage.getItem('JWT-token');
  }

  getRole(): string | null {
    return localStorage.getItem('user-role');
  }

  getUserName(): string | null {
    return localStorage.getItem('user-name');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
