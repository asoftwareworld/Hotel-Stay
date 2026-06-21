import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let detail: string;

      if (error.status === 0 || error.error instanceof ProgressEvent) {
        detail = 'Cannot reach the API. Make sure the backend is running on http://localhost:5000.';
      } else {
        // application/problem+json may arrive as a raw string — parse defensively
        let parsed: { detail?: string } | null = null;
        if (error.error && typeof error.error === 'object') {
          parsed = error.error as { detail?: string };
        } else if (typeof error.error === 'string') {
          try { parsed = JSON.parse(error.error) as { detail?: string }; } catch { /* raw text */ }
        }
        detail = parsed?.detail ?? error.message ?? 'An unexpected error occurred.';
      }

      return throwError(() => new Error(detail));
    })
  );
};
