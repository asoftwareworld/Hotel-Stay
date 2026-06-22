import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, TokenResponse } from '../models/auth.model';

const ACCESS_TOKEN_KEY = 'hs_access_token';
const REFRESH_TOKEN_KEY = 'hs_refresh_token';
const API = '/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _accessToken = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));

  readonly isAuthenticated = computed(() => this._accessToken() !== null);

  getAccessToken(): string | null {
    return this._accessToken();
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  register(body: RegisterRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${API}/register`, body).pipe(
      tap(res => this.storeTokens(res))
    );
  }

  login(body: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${API}/login`, body).pipe(
      tap(res => this.storeTokens(res))
    );
  }

  refresh(): Observable<TokenResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<TokenResponse>(`${API}/refresh`, { refreshToken }).pipe(
      tap(res => this.storeTokens(res))
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      // Fire-and-forget: best-effort server-side revocation
      this.http.post(`${API}/logout`, { refreshToken }).subscribe({ error: () => {} });
    }
    this.clearTokens();
    this.router.navigate(['/login']);
  }

  private storeTokens(res: TokenResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    this._accessToken.set(res.accessToken);
  }

  private clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this._accessToken.set(null);
  }
}
