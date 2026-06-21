import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let detail: string;

      if (error.status === 0 || error.error instanceof SyntaxError || error.error instanceof ProgressEvent) {
        // Network error or non-JSON response — backend is not running or proxy is misconfigured.
        detail = 'Cannot reach the API. Make sure the backend is running on http://localhost:5000.';
      } else {
        detail =
          (error.error as { detail?: string })?.detail ??
          error.message ??
          'An unexpected error occurred.';
      }

      return throwError(() => new Error(detail));
    })
  );
};
