import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '@core/auth/auth.service';
import { LayoutService } from '@core/services/layout-service';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroUserCircle, heroSun, heroMoon, heroComputerDesktop } from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [NgIcon, RouterLink, RouterLinkActive],
  viewProviders: [provideIcons({ heroUserCircle, heroSun, heroMoon, heroComputerDesktop })],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
  readonly layout = inject(LayoutService);
  readonly isMenuOpen = signal(false);
  readonly isUserMenuOpen = signal(false);

  get themeIcon(): string {
    const pref = this.layout.themePreference();
    const map = { light: 'heroSun', dark: 'heroMoon', system: 'heroComputerDesktop' } as const;
    return map[pref];
  }

  toggleMenu(): void {
    this.isMenuOpen.update((v) => !v);
    if (this.isMenuOpen()) this.isUserMenuOpen.set(false);
  }

  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen.update((v) => !v);
    if (this.isUserMenuOpen()) this.isMenuOpen.set(false);
  }

  logoutFromDrawer(): void {
    this.closeMenu();
    this.auth.logout();
  }

  loginFromDrawer(): void {
    this.closeMenu();
    this.auth.login();
  }
}
