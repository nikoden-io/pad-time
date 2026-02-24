import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService, LoginResponse } from 'angular-auth-oidc-client';
import { CurrentUser, UserRole, MemberCategory } from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly oidc = inject(OidcSecurityService);
  private readonly router = inject(Router);

  private readonly _isAuthenticated = signal(false);
  private readonly _currentUser = signal<CurrentUser | null>(null);
  private readonly _isLoading = signal(true);

  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();

  readonly isAdmin = computed(() => {
    const user = this._currentUser();
    return user?.role === 'admin_site' || user?.role === 'admin_global';
  });

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

  login(): void {
    this.oidc.authorize();
  }

  logout(): void {
    this.oidc.logoff().subscribe({
      next: () => {
        this._isAuthenticated.set(false);
        this._currentUser.set(null);
        this.router.navigate(['/']);
      },
    });
  }

  getAccessToken$() {
    return this.oidc.getAccessToken();
  }
}
