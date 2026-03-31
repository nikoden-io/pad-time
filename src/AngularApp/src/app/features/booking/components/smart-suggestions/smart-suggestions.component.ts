// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {
  ChangeDetectionStrategy, Component, DestroyRef,
  EventEmitter, inject, Output, signal,
} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {CommonModule} from '@angular/common';
import {TranslocoModule} from '@jsverse/transloco';
import {ApiService} from '@core/services';
import {SlotSuggestion} from '@core/models';

@Component({
  selector: 'app-smart-suggestions',
  standalone: true,
  imports: [CommonModule, TranslocoModule],
  templateUrl: './smart-suggestions.component.html',
  styleUrls: ['./smart-suggestions.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SmartSuggestionsComponent {
  @Output() suggestionSelected = new EventEmitter<SlotSuggestion>();

  readonly suggestions = signal<SlotSuggestion[]>([]);
  readonly loading = signal(true);
  readonly hasError = signal(false);
  readonly collapsed = signal(false);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.loadSuggestions();
  }

  toggleCollapsed() {
    this.collapsed.update(v => !v);
  }

  selectSuggestion(suggestion: SlotSuggestion) {
    this.suggestionSelected.emit(suggestion);
  }

  formatTime(utcIso: string): string {
    const d = new Date(utcIso);
    return d.toLocaleTimeString('fr-BE', {
      hour: '2-digit',
      minute: '2-digit',
      timeZone: 'Europe/Brussels',
    });
  }

  formatDate(utcIso: string): string {
    const d = new Date(utcIso);
    return d.toLocaleDateString('fr-BE', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
      timeZone: 'Europe/Brussels',
    });
  }

  private loadSuggestions() {
    this.api.getSlotSuggestions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.suggestions.set(res.suggestions ?? []);
        this.loading.set(false);
        if (res.fallbackUsed || !res.suggestions?.length) {
          this.hasError.set(true);
        }
      },
      error: () => {
        this.loading.set(false);
        this.hasError.set(true);
      },
    });
  }
}
