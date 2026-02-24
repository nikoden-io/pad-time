import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink],
  template: `
    <h2 class="title">Welcome back</h2>
    <p class="subtitle">Sign in to continue to Pad'Time</p>

    <button class="btn-primary" (click)="login()">Sign in</button>

    <div class="divider">
      <span>New to Pad'Time?</span>
    </div>

    <a routerLink="/auth/register" class="btn-outline">Create an account</a>
  `,
  styles: [`
    :host {
      display: block;
    }

    .title {
      color: #1a1a2e;
      font-size: 1.75rem;
      font-weight: 700;
      margin: 0 0 0.5rem;
    }

    .subtitle {
      color: #6b7280;
      font-size: 1rem;
      margin: 0 0 2rem;
    }

    .btn-primary {
      display: block;
      width: 100%;
      background: #4ade80;
      color: #1a1a2e;
      padding: 0.75rem;
      border: none;
      border-radius: 6px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      text-align: center;
      transition: background 0.15s ease;
    }

    .btn-primary:hover {
      background: #3bc76d;
    }

    .divider {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin: 1.75rem 0;
    }

    .divider::before,
    .divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: #d1d5db;
    }

    .divider span {
      color: #6b7280;
      font-size: 0.875rem;
      white-space: nowrap;
    }

    .btn-outline {
      display: block;
      width: 100%;
      padding: 0.75rem;
      border: 2px solid #4ade80;
      border-radius: 6px;
      font-size: 1rem;
      font-weight: 600;
      color: #4ade80;
      text-align: center;
      text-decoration: none;
      cursor: pointer;
      transition: background 0.15s ease, color 0.15s ease;
      box-sizing: border-box;
    }

    .btn-outline:hover {
      background: #4ade80;
      color: #1a1a2e;
    }
  `],
})
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  login(): void {
    this.auth.login();
  }
}
