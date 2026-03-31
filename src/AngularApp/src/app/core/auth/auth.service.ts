// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService, LoginResponse } from 'angular-auth-oidc-client';
import { CurrentUser, UserRole, MemberCategory } from '../models';

/**
 * Central authentication service that wraps the OIDC security layer.
 * Manages user authentication state, current user identity, and role-based access checks.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly oidc = inject(OidcSecurityService);
  private readonly router = inject(Router);

  private readonly _isAuthenticated = signal(false);
  private readonly _currentUser = signal<CurrentUser | null>(null);
  private readonly _isLoading = signal(true);

  /** Whether the current user is authenticated. */
  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  /** The currently authenticated user, or null if not logged in. */
  readonly currentUser = this._currentUser.asReadonly();
  /** Whether the initial authentication check is still in progress. */
  readonly isLoading = this._isLoading.asReadonly();

  /** Whether the current user holds a site-level or global admin role. */
  readonly isAdmin = computed(() => {
    const user = this._currentUser();
    return user?.role === 'admin_site' || user?.role === 'admin_global';
  });

  /** Whether the current user holds the global admin role. */
  readonly isGlobalAdmin = computed(() => {
    return this._currentUser()?.role === 'admin_global';
  });

  constructor() {
    this.initAuth();
  }

  private initAuth(): void {
    this.oidc.checkAuth().subscribe({
      next: (loginResponse: LoginResponse) => {
        this._isAuthenticated.set(loginResponse.isAuthenticated);
        if (loginResponse.isAuthenticated && loginResponse.userData) {
          this.setUserFromClaims(loginResponse.userData);
        }
        this._isLoading.set(false);
      },
      error: () => {
        this._isLoading.set(false);
      },
    });
  }

  private setUserFromClaims(claims: Record<string, unknown>): void {
    const user: CurrentUser = {
      subject: (claims['sub'] as string) ?? '',
      matricule: (claims['matricule'] as string) ?? '',
      category: (claims['member_category'] as MemberCategory) ?? 'free',
      role: (claims['role'] as UserRole) ?? 'user',
      siteId: (claims['site_id'] as string) ?? null,
    };
    this._currentUser.set(user);
  }

  /** Initiates the OIDC login redirect flow. */
  login(): void {
    this.oidc.authorize();
  }

  /** Logs the user out, clears local state, and navigates to the home page. */
  logout(): void {
    this.oidc.logoff().subscribe({
      next: () => {
        this._isAuthenticated.set(false);
        this._currentUser.set(null);
        this.router.navigate(['/']);
      },
    });
  }

  /** Returns an observable that emits the current OIDC access token. */
  getAccessToken$() {
    return this.oidc.getAccessToken();
  }
}