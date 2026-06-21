import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenResponse } from '../models/auth.model';

// Module-level state so concurrent 401s share a single refresh
let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<TokenResponse | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const authed = addToken(req, authService.getAccessToken());

  return next(authed).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || req.url.includes('/auth/')) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        // Another request already triggered refresh — wait for it
        return refreshDone$.pipe(
          filter(t => t !== null),
          take(1),
          switchMap(t => next(addToken(req, t!.accessToken)))
        );
      }

      isRefreshing = true;
      refreshDone$.next(null);

      return authService.refresh().pipe(
        switchMap(tokens => {
          isRefreshing = false;
          refreshDone$.next(tokens);
          return next(addToken(req, tokens.accessToken));
        }),
        catchError(refreshError => {
          isRefreshing = false;
          refreshDone$.next(null);
          // Refresh failed — session is dead, send to login
          authService.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function addToken(req: HttpRequest<unknown>, token: string | null): HttpRequest<unknown> {
  if (!token) return req;
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
