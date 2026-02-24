import {
  ChangeDetectionStrategy, Component, inject,
  OnInit, signal, HostListener, ElementRef,
} from '@angular/core';
import {TranslocoService} from '@jsverse/transloco';

interface Language {
  code: string;
  flag: string;
}

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'language-switcher.component.html',
  styleUrl: 'language-switcher.component.scss',
})
export class LanguageSwitcherComponent implements OnInit {
  readonly languages: Language[] = [
    {code: 'en', flag: '🇬🇧'},
    {code: 'fr', flag: '🇫🇷'},
    {code: 'nl', flag: '🇳🇱'},
    {code: 'de', flag: '🇩🇪'},
  ];

  readonly current = signal<Language>(this.languages[1]);
  readonly isOpen = signal(false);

  private readonly transloco = inject(TranslocoService);
  private readonly el = inject(ElementRef);

  ngOnInit(): void {
    const code = this.transloco.getActiveLang();
    const found = this.languages.find(l => l.code === code);
    if (found) this.current.set(found);
  }

  toggleOpen(): void {
    this.isOpen.update(v => !v);
  }

  select(lang: Language): void {
    this.current.set(lang);
    this.isOpen.set(false);
    this.transloco.setActiveLang(lang.code);
    localStorage.setItem('preferredLanguage', lang.code);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent): void {
    if (!this.el.nativeElement.contains(e.target)) {
      this.isOpen.set(false);
    }
  }
}
