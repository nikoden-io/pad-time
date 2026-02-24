import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="auth-layout">
      <!-- Branding panel -->
      <div class="branding-panel">
        <div class="branding-content">
          <h1 class="logo">Pad'Time</h1>
          <p class="tagline">Your padel court booking platform</p>

          <ul class="features">
            <li>
              <span class="feature-icon">&#9679;</span>
              Book courts in seconds
            </li>
            <li>
              <span class="feature-icon">&#9679;</span>
              Find players near you
            </li>
            <li>
              <span class="feature-icon">&#9679;</span>
              Track your matches
            </li>
          </ul>
        </div>

        <p class="copyright">&copy; {{ year }} Pad'Time. All rights reserved.</p>
      </div>

      <!-- Form panel -->
      <div class="form-panel">
        <div class="form-wrapper">
          <router-outlet />
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
    }

    .auth-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      min-height: 100vh;
    }

    /* ── Branding panel ── */
    .branding-panel {
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      padding: 3rem 2rem;
      position: relative;
    }

    .branding-content {
      max-width: 360px;
    }

    .logo {
      color: #4ade80;
      font-size: 2.5rem;
      font-weight: 700;
      margin: 0 0 0.5rem;
    }

    .tagline {
      color: rgba(255, 255, 255, 0.7);
      font-size: 1.125rem;
      margin: 0 0 2.5rem;
    }

    .features {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .features li {
      color: rgba(255, 255, 255, 0.85);
      font-size: 1rem;
      padding: 0.625rem 0;
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .feature-icon {
      color: #4ade80;
      font-size: 0.5rem;
    }

    .copyright {
      position: absolute;
      bottom: 1.5rem;
      color: rgba(255, 255, 255, 0.35);
      font-size: 0.75rem;
      margin: 0;
    }

    /* ── Form panel ── */
    .form-panel {
      background: #fafafa;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .form-wrapper {
      width: 100%;
      max-width: 440px;
    }

    /* ── Responsive: tablet / mobile ── */
    @media (max-width: 767px) {
      .auth-layout {
        grid-template-columns: 1fr;
      }

      .branding-panel {
        padding: 2rem 1.5rem;
      }

      .features {
        display: none;
      }

      .copyright {
        display: none;
      }

      .tagline {
        margin-bottom: 0;
      }

      .form-panel {
        padding: 2rem 1.5rem;
      }
    }
  `],
})
export class AuthLayoutComponent {
  readonly year = new Date().getFullYear();
}
