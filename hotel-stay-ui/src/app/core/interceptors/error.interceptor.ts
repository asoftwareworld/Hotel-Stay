import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const detail: string =
        (error.error as { detail?: string })?.detail ??
        error.message ??
        'An unexpected error occurred.';
      return throwError(() => new Error(detail));
    })
  );
};
