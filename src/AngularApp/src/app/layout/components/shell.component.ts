// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {Component, inject} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {NavbarComponent} from './navbar/navbar.component';
import {LayoutService} from '@core/services/layout-service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <div class="app-shell">
      <app-navbar/>
      <main class="main-content">
        <router-outlet/>
      </main>
      <footer class="app-footer">
        <span class="footer-brand">Pad'Time</span>
        <span class="footer-sep">&middot;</span>
        <span class="footer-version">v2.0.0</span>
        <span class="footer-ai">Pad'AI</span>
      </footer>
    </div>
  `,
  styles: [`
    .app-shell {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .main-content {
      flex: 1;
      padding: 0;
      max-width: var(--pt-content-max);
      margin: 0 auto;
      width: 100%;
    }

    .app-footer {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.4rem;
      padding: 0.8rem 1rem;
      font-size: 0.68rem;
      color: var(--p-text-muted-color);
      border-top: 1px solid var(--p-content-border-color);
    }

    .footer-brand {
      font-weight: 600;
    }

    .footer-version {
      font-family: 'DM Mono', monospace;
      opacity: 0.7;
    }

    .footer-ai {
      font-weight: 700;
      font-size: 0.6rem;
      padding: 0.1rem 0.4rem;
      border-radius: 10px;
      border: 1px solid var(--p-content-border-color);
      margin-left: 0.2rem;
    }
  `],
})
export class ShellComponent {
  // Inject to ensure LayoutService is initialized (theme applied on startup)
  private readonly layout = inject(LayoutService);
}