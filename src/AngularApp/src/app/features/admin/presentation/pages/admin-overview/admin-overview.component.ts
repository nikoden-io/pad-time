import {ChangeDetectionStrategy, Component, DestroyRef, inject, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {ApiService} from '@core/services';
import {Site, SiteAlert, SiteOverview} from '@core/models';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [RouterLink, PageShellComponent],
  templateUrl: './admin-overview.component.html',
  styleUrl: './admin-overview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminOverviewComponent {
  readonly sites = signal<Site[]>([]);
  readonly selectedSiteId = signal<string | null>(null);
  readonly overview = signal<SiteOverview | null>(null);
  readonly loading = signal(false);
  readonly sitesLoading = signal(true);
  readonly error = signal<string | null>(null);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.api.getSites().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        const sites = Array.isArray(res) ? res : res?.items ?? [];
        this.sites.set(sites);
        this.sitesLoading.set(false);
      },
      error: () => this.sitesLoading.set(false),
    });
  }

  onSiteChange(siteId: string): void {
    this.selectedSiteId.set(siteId);
    this.loadOverview(siteId);
  }

  loadOverview(siteId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.overview.set(null);

    this.api.getAdminOverview(siteId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.overview.set(data);
        this.loading.set(false);
      },
      error: (e: any) => {
        this.error.set(e?.error?.title ?? 'Impossible de charger l\'aperçu.');
        this.loading.set(false);
      },
    });
  }

  alertsByType(): {type: string; label: string; icon: string; color: string; alerts: SiteAlert[]}[] {
    const ov = this.overview();
    if (!ov) return [];

    const groups: Record<string, {label: string; icon: string; color: string; alerts: SiteAlert[]}> = {
      j1_unprocessed: {label: 'Matchs J-1 non traités', icon: '⏰', color: '#f97316', alerts: []},
      unpaid_participants: {label: 'Participants impayés', icon: '💸', color: '#f59e0b', alerts: []},
      organizer_debt: {label: 'Dettes organisateurs', icon: '⚠️', color: '#f87171', alerts: []},
    };

    for (const alert of ov.alerts) {
      if (groups[alert.type]) {
        groups[alert.type].alerts.push(alert);
      } else {
        if (!groups['other']) {
          groups['other'] = {label: 'Autres alertes', icon: 'ℹ️', color: '#94a3b8', alerts: []};
        }
        groups['other'].alerts.push(alert);
      }
    }

    return Object.entries(groups)
      .filter(([, g]) => g.alerts.length > 0)
      .map(([type, g]) => ({type, ...g}));
  }

  formatPayload(payload: Record<string, any> | null): string {
    if (!payload) return '';
    const parts: string[] = [];
    if (payload['matchId']) parts.push(`Match: ${payload['matchId']}`);
    if (payload['memberId']) parts.push(`Membre: ${payload['memberId']}`);
    if (payload['amountCents'] != null) parts.push(`Montant: ${(payload['amountCents'] / 100).toFixed(2)} €`);
    if (payload['unpaidCount'] != null) parts.push(`Impayés: ${payload['unpaidCount']}`);
    if (payload['scheduledAt']) {
      parts.push(`Planifié: ${new Date(payload['scheduledAt']).toLocaleString('fr-BE')}`);
    }
    return parts.join(' · ');
  }
}
