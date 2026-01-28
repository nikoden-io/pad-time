import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="navbar-brand">
        <a routerLink="/" class="brand-link">Pad'Time</a>
      </div>

      <div class="navbar-menu">
        @if (auth.isAuthenticated()) {
          <a routerLink="/booking" routerLinkActive="active" class="nav-link">
            Booking
          </a>
          <a routerLink="/matches" routerLinkActive="active" class="nav-link">
            My Matches
          </a>
          @if (auth.isAdmin()) {
            <a routerLink="/admin" routerLinkActive="active" class="nav-link nav-link--admin">
              Admin
            </a>
          }
        }
      </div>

      <div class="navbar-end">
        @if (auth.isLoading()) {
          <span class="loading">Loading...</span>
        } @else if (auth.isAuthenticated()) {
          <div class="user-menu">
            <span class="user-info">
              {{ auth.currentUser()?.matricule }}
              <span class="user-role">({{ auth.currentUser()?.role }})</span>
            </span>
            <button class="btn btn-outline" (click)="auth.logout()">
              Logout
            </button>
          </div>
        } @else {
          <button class="btn btn-primary" (click)="auth.login()">
            Login
          </button>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1rem 2rem;
      background: #1a1a2e;
      color: white;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .brand-link {
      font-size: 1.5rem;
      font-weight: bold;
      color: #4ade80;
      text-decoration: none;
    }

    .navbar-menu {
      display: flex;
      gap: 1.5rem;
    }

    .nav-link {
      color: #e0e0e0;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background-color 0.2s;

      &:hover {
        background: rgba(255, 255, 255, 0.1);
      }

      &.active {
        background: rgba(74, 222, 128, 0.2);
        color: #4ade80;
      }
    }

    .nav-link--admin {
      color: #fbbf24;

      &.active {
        background: rgba(251, 191, 36, 0.2);
        color: #fbbf24;
      }
    }

    .navbar-end {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-menu {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-info {
      font-size: 0.875rem;
    }

    .user-role {
      color: #9ca3af;
      font-size: 0.75rem;
    }

    .btn {
      padding: 0.5rem 1rem;
      border-radius: 4px;
      border: none;
      cursor: pointer;
      font-weight: 500;
      transition: opacity 0.2s;

      &:hover {
        opacity: 0.9;
      }
    }

    .btn-primary {
      background: #4ade80;
      color: #1a1a2e;
    }

    .btn-outline {
      background: transparent;
      border: 1px solid #4ade80;
      color: #4ade80;
    }

    .loading {
      color: #9ca3af;
      font-size: 0.875rem;
    }
  `],
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
}
