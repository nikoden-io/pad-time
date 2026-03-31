// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {ChangeDetectionStrategy, Component, DestroyRef, inject, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {TranslocoDirective, TranslocoService} from '@jsverse/transloco';
import {ApiService} from '@core/services';
import {RevenueAnalytics, RevenueItem, Site} from '@core/models';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [RouterLink, TranslocoDirective, PageShellComponent],
  templateUrl: './admin-analytics.component.html',
  styleUrl: './admin-analytics.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminAnalyticsComponent {
  readonly sites = signal<Site[]>([]);
  readonly sitesLoading = signal(true);
  readonly selectedSiteId = signal<string>('');

  readonly fromDate = signal<string>(this.defaultFrom());
  readonly toDate = signal<string>(this.defaultTo());

  readonly analytics = signal<RevenueAnalytics | null>(null);
  readonly loading = signal(false);
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

  onFromChange(value: string): void { this.fromDate.set(value); }
  onToChange(value: string): void { this.toDate.set(value); }
  onSiteChange(value: string): void { this.selectedSiteId.set(value); }

  load(): void {
    const from = new Date(this.fromDate());
    const to = new Date(this.toDate());
    to.setHours(23, 59, 59, 999);

    this.loading.set(true);
    this.error.set(null);
    this.analytics.set(null);

    const params: {siteId?: string; from: string; to: string} = {
      from: from.toISOString(),
      to: to.toISOString(),
    };
    if (this.selectedSiteId()) params['siteId'] = this.selectedSiteId();

    this.api.getAdminRevenue(params).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.analytics.set(data);
        this.loading.set(false);
      },
      error: (e: any) => {
        this.error.set(e?.error?.title ?? this.transloco.translate('admin.analytics.loadError'));
        this.loading.set(false);
      },
    });
  }

  totalCents(): number {
    return this.analytics()?.items.reduce((s, i) => s + i.amountCents, 0) ?? 0;
  }

  totalPayments(): number {
    return this.analytics()?.items.reduce((s, i) => s + i.paymentCount, 0) ?? 0;
  }

  formatEuros(cents: number): string {
    return (cents / 100).toLocaleString('fr-BE', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: 2,
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('fr-BE', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  siteName(siteId: string): string {
    return this.sites().find(s => s.siteId === siteId)?.name ?? siteId;
  }

  trackItem(_i: number, item: RevenueItem): string {
    return item.date + item.siteId;
  }

  private defaultFrom(): string {
    const d = new Date();
    d.setDate(d.getDate() - 29);
    return d.toISOString().slice(0, 10);
  }

  private defaultTo(): string {
    return new Date().toISOString().slice(0, 10);
  }
}