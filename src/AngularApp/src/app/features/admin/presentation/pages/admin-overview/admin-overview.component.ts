import {ChangeDetectionStrategy, Component, DestroyRef, inject, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {TranslocoDirective, TranslocoService} from '@jsverse/transloco';
import {ApiService} from '@core/services';
import {Site, SiteAlert, SiteOverview} from '@core/models';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [RouterLink, TranslocoDirective, PageShellComponent],
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
  private readonly transloco = inject(TranslocoService);

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
        this.error.set(e?.error?.title ?? this.transloco.translate('admin.overview.loadError'));
        this.loading.set(false);
      },
    });
  }

  alertsByType(): {type: string; label: string; icon: string; color: string; alerts: SiteAlert[]}[] {
    const ov = this.overview();
    if (!ov) return [];

    const t = (key: string) => this.transloco.translate(key);

    const groups: Record<string, {label: string; icon: string; color: string; alerts: SiteAlert[]}> = {
      j1_unprocessed: {label: t('admin.overview.alertTypes.j1Unprocessed'), icon: '⏰', color: '#f97316', alerts: []},
      unpaid_participants: {label: t('admin.overview.alertTypes.unpaidParticipants'), icon: '💸', color: '#f59e0b', alerts: []},
      organizer_debt: {label: t('admin.overview.alertTypes.organizerDebt'), icon: '⚠️', color: '#f87171', alerts: []},
    };

    for (const alert of ov.alerts) {
      if (groups[alert.type]) {
        groups[alert.type].alerts.push(alert);
      } else {
        if (!groups['other']) {
          groups['other'] = {label: t('admin.overview.alertTypes.other'), icon: 'ℹ️', color: '#94a3b8', alerts: []};
        }
        groups['other'].alerts.push(alert);
      }
    }

    return Object.entries(groups)
      .filter(([, g]) => g.alerts.length > 0)
      .map(([type, g]) => ({type, ...g}));
  }

  translateAlert(alert: SiteAlert): string {
    const t = (key: string, params?: Record<string, any>) => this.transloco.translate(key, params);
    const payload = alert.payload;

    switch (alert.type) {
      case 'j1_unprocessed':
        return t('admin.overview.alerts.j1Unprocessed');
      case 'unpaid_participants':
        return t('admin.overview.alerts.unpaidParticipants', { count: payload?.['unpaidCount'] ?? 0 });
      case 'organizer_debt':
        return t('admin.overview.alerts.organizerDebt', { amount: ((payload?.['amountCents'] ?? 0) / 100).toFixed(2) });
      default:
        return alert.description;
    }
  }

  formatPayload(payload: Record<string, any> | null): string {
    if (!payload) return '';
    const t = (key: string) => this.transloco.translate(key);
    const lang = this.transloco.getActiveLang();
    const locale = lang === 'fr' ? 'fr-BE' : lang === 'nl' ? 'nl-BE' : lang === 'de' ? 'de-DE' : 'en-GB';
    const parts: string[] = [];
    if (payload['matchId']) parts.push(`${t('admin.overview.payload.match')}: ${payload['matchId']}`);
    if (payload['memberId']) parts.push(`${t('admin.overview.payload.member')}: ${payload['memberId']}`);
    if (payload['amountCents'] != null) parts.push(`${t('admin.overview.payload.amount')}: ${(payload['amountCents'] / 100).toFixed(2)} €`);
    if (payload['unpaidCount'] != null) parts.push(`${t('admin.overview.payload.unpaid')}: ${payload['unpaidCount']}`);
    if (payload['scheduledAt']) {
      parts.push(`${t('admin.overview.payload.scheduled')}: ${new Date(payload['scheduledAt']).toLocaleString(locale)}`);
    }
    return parts.join(' · ');
  }
}
