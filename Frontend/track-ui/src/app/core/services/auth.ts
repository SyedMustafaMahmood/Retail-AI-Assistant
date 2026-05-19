import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { User } from '../../shared/models/user';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: {
    id: number;
    name: string;
    email: string;
    role: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_KEY = 'refresh_token';
  private readonly USER_KEY = 'auth_user';

  private currentUserSubject = new BehaviorSubject<User | null>(
    this.getUserFromStorage()
  );

  currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${environment.apiUrl}/v2/auth/login`,
        request
      )
      .pipe(
        tap(response => this.handleAuthResponse(response))
      );
  }

  register(request: RegisterRequest): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}/v2/auth/register`,
      request
    );
  }

  refreshToken(): Observable<any> {
    return this.http.post(
      `${environment.apiUrl}/v2/auth/refresh`,
      {
        token: this.getRefreshToken()
      }
    );
  }

  logout(): void {

    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);

    this.currentUserSubject.next(null);

    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  getRole(): string | null {
    return this.currentUserSubject.value?.role ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private handleAuthResponse(response: AuthResponse): void {

    localStorage.setItem(
      this.TOKEN_KEY,
      response.accessToken
    );

    localStorage.setItem(
      this.REFRESH_KEY,
      response.refreshToken
    );

    const user: User = {
      id: response.user.id,
      name: response.user.name,
      email: response.user.email,
      role: response.user.role as User['role']
    };

    localStorage.setItem(
      this.USER_KEY,
      JSON.stringify(user)
    );

    this.currentUserSubject.next(user);
  }

  private getUserFromStorage(): User | null {

    const user = localStorage.getItem(this.USER_KEY);

    return user
      ? JSON.parse(user)
      : null;
  }
}