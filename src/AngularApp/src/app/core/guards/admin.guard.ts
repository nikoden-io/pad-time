// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Route guard that restricts access to users with a site-level or global admin role.
 * Redirects non-admin users to the home page.
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  // Redirect to home if not admin
  router.navigate(['/']);
  return false;
};

/**
 * Route guard that restricts access to users with the global admin role only.
 * Redirects non-global-admin users to the home page.
 */
export const globalAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isGlobalAdmin()) {
    return true;
  }

  // Redirect to home if not global admin
  router.navigate(['/']);
  return false;
};