import {Component, inject, signal, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {TranslocoService} from '@jsverse/transloco';
import {Select} from 'primeng/select';
import {FormsModule} from '@angular/forms';

interface Language {
  code: string;
  label: string;
  flag: string;
}

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, Select, FormsModule],
  templateUrl: './language-switcher.component.html',
  styleUrl: './language-switcher.component.scss',
})
export class LanguageSwitcherComponent implements OnInit {
  readonly languages: Language[] = [
    {code: 'en', label: 'English', flag: '🇬🇧'},
    {code: 'fr', label: 'Français', flag: '🇫🇷'},
    {code: 'nl', label: 'Nederlands', flag: '🇳🇱'},
    {code: 'de', label: 'Deutsch', flag: '🇩🇪'},
  ];
  selectedLanguage = signal<Language>(this.languages[0]);
  private readonly transloco = inject(TranslocoService);

  ngOnInit(): void {
    const activeLang = this.transloco.getActiveLang();
    const language = this.languages.find(lang => lang.code === activeLang);
    if (language) {
      this.selectedLanguage.set(language);
    }
  }

  onLanguageChange(language: Language): void {
    this.selectedLanguage.set(language);
    this.transloco.setActiveLang(language.code);
    localStorage.setItem('preferredLanguage', language.code);
  }
}
