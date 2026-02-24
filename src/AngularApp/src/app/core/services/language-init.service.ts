import {Injectable, inject} from '@angular/core';
import {TranslocoService} from '@jsverse/transloco';

@Injectable({providedIn: 'root'})
export class LanguageInitService {
  private readonly transloco = inject(TranslocoService);
  private readonly STORAGE_KEY = 'preferredLanguage';
  private readonly SUPPORTED_LANGS = ['en', 'fr', 'nl', 'de'];

  initialize(): void {
    const savedLang = localStorage.getItem(this.STORAGE_KEY);

    if (savedLang && this.SUPPORTED_LANGS.includes(savedLang)) {
      this.transloco.setActiveLang(savedLang);
      return;
    }

    const browserLang = this.detectBrowserLanguage();
    this.transloco.setActiveLang(browserLang);
    localStorage.setItem(this.STORAGE_KEY, browserLang);
  }

  private detectBrowserLanguage(): string {
    const browserLangs = navigator.languages || [navigator.language];

    for (const browserLang of browserLangs) {
      const langCode = browserLang.split('-')[0].toLowerCase();

      if (this.SUPPORTED_LANGS.includes(langCode)) {
        return langCode;
      }
    }

    return 'en';
  }
}
