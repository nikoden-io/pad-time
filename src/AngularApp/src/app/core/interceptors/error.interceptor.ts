import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models';

// Track if we're already redirecting to prevent loops
let isRedirecting = false;

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Only redirect if not already on auth pages and not already redirecting
        const currentUrl = router.url;
        const isOnAuthPage = currentUrl.startsWith('/auth') || currentUrl.startsWith('/callback');

        if (!isOnAuthPage && !isRedirecting) {
          isRedirecting = true;
          router.navigate(['/auth/login']).then(() => {
            // Reset flag after navigation completes
            setTimeout(() => { isRedirecting = false; }, 1000);
          });
        }
      } else if (error.status === 403) {
        // Forbidden - could show a notification or redirect
        console.error('Access forbidden:', error.error);
      }

      // Try to parse as ProblemDetails
      const problemDetails = error.error as ProblemDetails;
      if (problemDetails?.type) {
        console.error(`API Error [${problemDetails.type}]:`, problemDetails.detail);
      }

      return throwError(() => error);
    })
  );
};
