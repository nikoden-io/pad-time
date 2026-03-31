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
  `],
})
export class ShellComponent {
  // Inject to ensure LayoutService is initialized (theme applied on startup)
  private readonly layout = inject(LayoutService);
}