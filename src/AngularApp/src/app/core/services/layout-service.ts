import {computed, effect, Injectable, signal} from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'pad-time-theme';

@Injectable({providedIn: 'root'})
export class LayoutService {
  readonly themePreference = signal<ThemeMode>(this.loadPreference());
  readonly themeLabel = computed<string>(() => {
    const map: Record<ThemeMode, string> = {light: 'Light', dark: 'Dark', system: 'System'};
    return map[this.themePreference()];
  });
  private readonly systemPrefersDark = signal(this.getSystemPreference());
  readonly isDark = computed(() => {
    const pref = this.themePreference();
    if (pref === 'system') return this.systemPrefersDark();
    return pref === 'dark';
  });

  constructor() {
    // Listen to OS dark mode changes
    const mql = window.matchMedia('(prefers-color-scheme: dark)');
    mql.addEventListener('change', (e) => this.systemPrefersDark.set(e.matches));

    effect(() => {
      const dark = this.isDark();
      this.applyTheme(dark);
    });

    effect(() => {
      localStorage.setItem(STORAGE_KEY, this.themePreference());
    });
  }

  /** Cycle through: light → dark → system */
  toggleTheme(): void {
    const cycle: Record<ThemeMode, ThemeMode> = {
      light: 'dark',
      dark: 'system',
      system: 'light',
    };
    this.themePreference.set(cycle[this.themePreference()]);
  }

  setTheme(mode: ThemeMode): void {
    this.themePreference.set(mode);
  }

  private applyTheme(dark: boolean): void {
    const el = document.documentElement;
    if (dark) {
      el.classList.add('p-dark');
    } else {
      el.classList.remove('p-dark');
    }
  }

  private loadPreference(): ThemeMode {
    const stored = localStorage.getItem(STORAGE_KEY) as ThemeMode | null;
    if (stored && ['light', 'dark', 'system'].includes(stored)) return stored;
    return 'system';
  }

  private getSystemPreference(): boolean {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  }
}
