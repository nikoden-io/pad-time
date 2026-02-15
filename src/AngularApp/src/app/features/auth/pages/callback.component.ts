import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div class="callback-container">
      <div class="callback-content">
        <h1 class="logo">Pad'Time</h1>
        <div class="spinner"></div>
        <p class="message">Authenticating...</p>
      </div>
    </div>
  `,
  styles: [`
    .callback-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
    }

    .callback-content {
      text-align: center;
    }

    .logo {
      color: #4ade80;
      font-size: 2rem;
      font-weight: 700;
      margin: 0 0 2rem;
    }

    .spinner {
      width: 2.5rem;
      height: 2.5rem;
      border: 3px solid rgba(74, 222, 128, 0.2);
      border-top-color: #4ade80;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 1.5rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .message {
      color: rgba(255, 255, 255, 0.6);
      font-size: 0.875rem;
      margin: 0;
    }
  `],
})
export class CallbackComponent implements OnInit {
  private readonly oidc = inject(OidcSecurityService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.oidc.checkAuth().subscribe({
      next: (result) => {
        if (result.isAuthenticated) {
          this.router.navigate(['/']);
        } else {
          this.router.navigate(['/auth/login']);
        }
      },
      error: () => {
        this.router.navigate(['/auth/login']);
      },
    });
  }
}
