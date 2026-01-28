import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>Pad'Time</h1>
        <p>Book your padel courts</p>
        <button class="btn btn-primary" (click)="login()">
          Sign in with your account
        </button>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
    }

    .login-card {
      background: white;
      padding: 3rem;
      border-radius: 8px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      text-align: center;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 0.5rem;
    }

    p {
      color: #6b7280;
      margin-bottom: 2rem;
    }

    .btn-primary {
      background: #4ade80;
      color: #1a1a2e;
      padding: 0.75rem 2rem;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
    }

    .btn-primary:hover {
      background: #3bc76d;
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
