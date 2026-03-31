// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '@core/services/account.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h2 class="title">Create your account</h2>
    <p class="subtitle">Join Pad'Time to book your padel courts</p>

    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <div class="name-row">
        <div class="form-group">
          <label for="firstName">First Name</label>
          <input id="firstName" type="text" formControlName="firstName" placeholder="John" />
          @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
            <span class="error">First name is required</span>
          }
        </div>

        <div class="form-group">
          <label for="lastName">Last Name</label>
          <input id="lastName" type="text" formControlName="lastName" placeholder="Doe" />
          @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
            <span class="error">Last name is required</span>
          }
        </div>
      </div>

      <div class="form-group">
        <label for="email">Email</label>
        <input id="email" type="email" formControlName="email" placeholder="john.doe&#64;example.com" />
        @if (form.get('email')?.invalid && form.get('email')?.touched) {
          <span class="error">Valid email is required</span>
        }
      </div>

      <div class="form-group">
        <label for="password">Password</label>
        <input id="password" type="password" formControlName="password" placeholder="Min. 8 characters" />
        @if (form.get('password')?.invalid && form.get('password')?.touched) {
          <span class="error">Password must be at least 8 characters</span>
        }
      </div>

      <div class="form-group">
        <label for="confirmPassword">Confirm Password</label>
        <input id="confirmPassword" type="password" formControlName="confirmPassword" placeholder="Repeat your password" />
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

      <button type="submit" class="btn-primary" [disabled]="form.invalid || isLoading()">
        @if (isLoading()) {
          <span class="spinner"></span>
          <span>Creating account...</span>
        } @else {
          <span>Create Account</span>
        }
      </button>

      <p class="signin-link">
        Already have an account? <a routerLink="/auth/login">Sign in</a>
      </p>
    </form>
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
      margin: 0 0 1.75rem;
    }

    .name-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
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
      padding: 0.625rem 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 0.875rem;
      box-sizing: border-box;
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

    input::placeholder {
      color: #9ca3af;
    }

    input:focus {
      outline: none;
      border-color: #4ade80;
      box-shadow: 0 0 0 3px rgba(74, 222, 128, 0.15);
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
      border-radius: 6px;
      margin-bottom: 1rem;
      font-size: 0.875rem;
    }

    .alert-success {
      background: #d1fae5;
      color: #065f46;
      padding: 0.75rem;
      border-radius: 6px;
      margin-bottom: 1rem;
      font-size: 0.875rem;
    }

    .btn-primary {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      width: 100%;
      background: #4ade80;
      color: #1a1a2e;
      padding: 0.75rem;
      border: none;
      border-radius: 6px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.15s ease;
    }

    .btn-primary:hover:not(:disabled) {
      background: #3bc76d;
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .spinner {
      width: 1rem;
      height: 1rem;
      border: 2px solid #1a1a2e;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .signin-link {
      text-align: center;
      font-size: 0.875rem;
      color: #6b7280;
      margin: 1.25rem 0 0;
    }

    .signin-link a {
      color: #4ade80;
      text-decoration: none;
      font-weight: 500;
    }

    .signin-link a:hover {
      text-decoration: underline;
    }

    /* Stack name fields on very small screens */
    @media (max-width: 479px) {
      .name-row {
        grid-template-columns: 1fr;
      }
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