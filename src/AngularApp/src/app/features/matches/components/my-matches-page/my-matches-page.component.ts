import {
  ChangeDetectionStrategy, Component, DestroyRef,
  inject, signal, computed,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {forkJoin} from 'rxjs';
import {ApiService} from '@core/services';
import {Match} from '@core/models';
import {MatchCardComponent} from '@features/matches/components/match-card/match-card.component';

export type MatchFilter = 'all' | 'upcoming' | 'past' | 'public' | 'private';

@Component({
  selector: 'app-my-matches-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MatchCardComponent],
  templateUrl: './my-matches-page.component.html',
  styleUrls: ['./my-matches-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyMatchesPageComponent {
  readonly loading = signal(true);
  readonly matches = signal<Match[]>([]);
  readonly sitesMap = signal<Record<string, string>>({});
  readonly filter = signal<MatchFilter>('all');
  readonly filters: {label: string; value: MatchFilter}[] = [
    {label: 'Tous', value: 'all'},
    {label: 'À venir', value: 'upcoming'},
    {label: 'Passés', value: 'past'},
    {label: 'Public', value: 'public'},
    {label: 'Privé', value: 'private'},
  ];
  readonly filtered = computed(() => {
    const now = new Date();
    return this.matches().filter(m => {
      switch (this.filter()) {
        case 'upcoming': return new Date(m.startAtUtc) > now;
        case 'past':     return new Date(m.startAtUtc) <= now;
        case 'public':   return m.type === 'public';
        case 'private':  return m.type === 'private';
        default:         return true;
      }
    });
  });

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.load();
  }

  setFilter(f: MatchFilter) {
    this.filter.set(f);
  }

  onPaymentDone() {
    this.load();
  }

  load() {
    forkJoin({
      matches: this.api.getUserMatches(),
      sites: this.api.getSites(),
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({matches, sites}: any) => {
        this.matches.set(Array.isArray(matches) ? matches : matches?.items ?? []);
        const siteList = Array.isArray(sites) ? sites : sites?.items ?? [];
        this.sitesMap.set(Object.fromEntries(siteList.map((s: any) => [s.siteId, s.name])));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
