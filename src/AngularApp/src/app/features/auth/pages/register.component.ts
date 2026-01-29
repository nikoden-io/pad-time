import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AccountService } from '@core/services/account.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="register-container">
      <div class="register-card">
        <h1>Create Account</h1>
        <p>Join Pad'Time to book your padel courts</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>First Name</label>
            <input type="text" formControlName="firstName" />
            @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
              <span class="error">First name is required</span>
            }
          </div>

          <div class="form-group">
            <label>Last Name</label>
            <input type="text" formControlName="lastName" />
            @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
              <span class="error">Last name is required</span>
            }
          </div>

          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" />
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <span class="error">Valid email is required</span>
            }
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="error">Password must be at least 8 characters</span>
            }
          </div>

          <div class="form-group">
            <label>Confirm Password</label>
            <input type="password" formControlName="confirmPassword" />
            @if (form.get('confirmPassword')?.invalid && form.get('confirmPassword')?.touched) {
              <span class="error">Passwords must match</span>
            }
          </div>

          @if (errorMessage()) {
            <div class="alert-error">{{ errorMessage() }}</div>
          }

          @if (successMessage()) {
            <div class="alert-success">{{ successMessage() }}</div>
          }

          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || isLoading()">
            @if (isLoading()) {
              <span>Creating account...</span>
            } @else {
              <span>Create Account</span>
            }
          </button>

          <p class="text-center">
            Already have an account? <a routerLink="/auth/login">Sign in</a>
          </p>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
      padding: 1rem;
    }

    .register-card {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 400px;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 0.5rem;
      text-align: center;
    }

    p {
      color: #6b7280;
      margin-bottom: 1.5rem;
      text-align: center;
    }

    .form-group {
      margin-bottom: 1rem;
    }

    label {
      display: block;
      color: #374151;
      font-weight: 500;
      margin-bottom: 0.25rem;
      font-size: 0.875rem;
    }

    input {
      width: 100%;
      padding: 0.625rem;
      border: 1px solid #d1d5db;
      border-radius: 4px;
      font-size: 0.875rem;
    }

    input:focus {
      outline: none;
      border-color: #4ade80;
    }

    .error {
      color: #ef4444;
      font-size: 0.75rem;
      margin-top: 0.25rem;
      display: block;
    }

    .alert-error {
      background: #fee2e2;
      color: #991b1b;
      padding: 0.75rem;
      border-radius: 4px;
      margin-bottom: 1rem;
      font-size: 0.875rem;
    }

    .alert-success {
      background: #d1fae5;
      color: #065f46;
      padding: 0.75rem;
      border-radius: 4px;
      margin-bottom: 1rem;
      font-size: 0.875rem;
    }

    .btn-primary {
      width: 100%;
      background: #4ade80;
      color: #1a1a2e;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      font-weight: 500;
      cursor: pointer;
      margin-bottom: 1rem;
    }

    .btn-primary:hover:not(:disabled) {
      background: #3bc76d;
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .text-center {
      text-align: center;
      font-size: 0.875rem;
    }

    a {
      color: #4ade80;
      text-decoration: none;
      font-weight: 500;
    }

    a:hover {
      text-decoration: underline;
    }
  `],
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly form: FormGroup = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  }, {
    validators: this.passwordMatchValidator
  });

  private passwordMatchValidator(group: FormGroup) {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const request = this.form.value;

    this.accountService.register(request).subscribe({
      next: (response) => {
        this.successMessage.set(`Account created! Your member number is ${response.matricule}`);
        this.isLoading.set(false);
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2000);
      },
      error: (error) => {
        this.errorMessage.set(error.error?.message || 'Registration failed. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
