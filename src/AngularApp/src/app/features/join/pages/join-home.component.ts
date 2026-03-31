// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {
  ChangeDetectionStrategy, Component, DestroyRef,
  computed, inject, signal,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {DatePickerModule} from 'primeng/datepicker';
import {ApiService} from '@core/services';
import {Match, Site} from '@core/models';
import {PublicMatchCardComponent} from '../components/public-match-card/public-match-card.component';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-join-home',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePickerModule, PublicMatchCardComponent, PageShellComponent],
  templateUrl: './join-home.component.html',
  styleUrls: ['./join-home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JoinHomeComponent {
  // ── State ──────────────────────────────────────────
  readonly sites = signal<Site[]>([]);
  readonly matches = signal<Match[]>([]);
  readonly sitesMap = signal<Record<string, string>>({});
  readonly loading = signal(false);
  readonly loadingMore = signal(false);
  readonly error = signal<string | null>(null);

  readonly selectedSiteId = signal<string | null>(null);
  readonly fromDate = signal<Date>(new Date());
  readonly toDate = signal<Date>((() => { const d = new Date(); d.setDate(d.getDate() + 14); return d; })());

  readonly page = signal(1);
  readonly hasMore = signal(false);
  private readonly PAGE_SIZE = 10;

  // ── Derived ────────────────────────────────────────
  readonly filteredMatches = computed(() =>
    this.matches().filter(m => m.status === 'public' || m.status === 'full')
  );

  readonly availableMatches = computed(() =>
    this.filteredMatches().filter(m => m.status === 'public')
  );

  readonly fullMatches = computed(() =>
    this.filteredMatches().filter(m => m.status === 'full')
  );

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.loadSites();
    this.loadMatches();
  }

  // ── Handlers ───────────────────────────────────────
  onSiteChange(siteId: string | null) {
    this.selectedSiteId.set(siteId);
    this.resetAndLoad();
  }

  onFromDateChange(d: Date | null) {
    if (d) {
      this.fromDate.set(d);
      this.resetAndLoad();
    }
  }

  onToDateChange(d: Date | null) {
    if (d) {
      this.toDate.set(d);
      this.resetAndLoad();
    }
  }

  onJoined(matchId: string) {
    // Update local state: increment paidCount by adding a fake paid participant
    // so the card reflects the change without reloading
    this.matches.update(list =>
      list.map(m => {
        if (m.matchId !== matchId) return m;
        const updated: Match = {
          ...m,
          participants: [
            ...m.participants,
            {memberId: 'me', matricule: '—', role: 'player', paymentStatus: 'paid'},
          ],
        };
        // transition to full if 4 paid
        const paidCount = updated.participants.filter(p => p.paymentStatus === 'paid').length;
        return {...updated, status: paidCount >= 4 ? 'full' : updated.status};
      })
    );
  }

  loadMore() {
    this.page.update(p => p + 1);
    this.loadMatchesPage(this.page(), true);
  }

  // ── Private ────────────────────────────────────────
  private resetAndLoad() {
    this.page.set(1);
    this.matches.set([]);
    this.loadMatches();
  }

  private loadSites() {
    this.api.getSites().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        const list: Site[] = Array.isArray(res) ? res : res?.items ?? [];
        this.sites.set(list);
        this.sitesMap.set(Object.fromEntries(list.map(s => [s.siteId, s.name])));
      },
    });
  }

  private loadMatches() {
    this.loadMatchesPage(1, false);
  }

  private loadMatchesPage(page: number, append: boolean) {
    if (append) {
      this.loadingMore.set(true);
    } else {
      this.loading.set(true);
      this.error.set(null);
    }

    this.api.getPublicMatches({
      siteId: this.selectedSiteId() ?? undefined,
      fromUtc: this.fromDate().toISOString(),
      toUtc: this.toDate().toISOString(),
      page,
      pageSize: this.PAGE_SIZE,
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        const items: Match[] = Array.isArray(res) ? res : res?.items ?? [];
        this.matches.update(current => append ? [...current, ...items] : items);
        this.hasMore.set(items.length === this.PAGE_SIZE);
        this.loading.set(false);
        this.loadingMore.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger les matchs disponibles.');
        this.loading.set(false);
        this.loadingMore.set(false);
      },
    });
  }

  siteNameFor(siteId: string): string {
    return this.sitesMap()[siteId] ?? siteId;
  }

  private addDays(d: Date, n: number): Date {
    const x = new Date(d);
    x.setDate(x.getDate() + n);
    return x;
  }
}