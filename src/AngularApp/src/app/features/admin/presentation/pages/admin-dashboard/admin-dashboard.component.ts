// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {RouterLink} from '@angular/router';
import {forkJoin} from 'rxjs';
import {TranslocoDirective, TranslocoService} from '@jsverse/transloco';
import {CommonModule} from '@angular/common';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';
import {ApiService} from '@core/services';
import {AiTrend} from '@core/models';

interface KpiCard {
  label: string;
  value: string;
  delta?: string;
  deltaDown?: boolean;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslocoDirective, PageShellComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardComponent implements OnInit {
  readonly kpis = signal<KpiCard[]>([]);
  readonly aiTrends = signal<AiTrend[]>([]);
  readonly trendsLoading = signal(true);
  readonly trendsFallback = signal(false);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly transloco = inject(TranslocoService);

  ngOnInit(): void {
    this.kpis.set([
      {label: this.transloco.translate('admin.dashboard.kpis.activeSites'), value: '—'},
      {label: this.transloco.translate('admin.dashboard.kpis.matchesToday'), value: '—'},
      {label: this.transloco.translate('admin.dashboard.kpis.revenueToday'), value: '—'},
      {label: this.transloco.translate('admin.dashboard.kpis.activeDebts'), value: '—'},
    ]);

    // Load AI trends
    this.api.getAiTrends().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.aiTrends.set(res.trends ?? []);
        this.trendsFallback.set(res.fallbackUsed);
        this.trendsLoading.set(false);
      },
      error: () => {
        this.trendsLoading.set(false);
        this.trendsFallback.set(true);
      },
    });

    const todayStart = new Date();
    todayStart.setHours(0, 0, 0, 0);
    const tomorrowStart = new Date(todayStart);
    tomorrowStart.setDate(tomorrowStart.getDate() + 1);

    forkJoin({
      sites: this.api.getSites(),
      matches: this.api.getMatches({
        scope: 'public',
        from: todayStart.toISOString(),
        to: tomorrowStart.toISOString(),
        pageSize: 1,
      }),
      revenue: this.api.getAdminRevenue({
        from: todayStart.toISOString(),
        to: tomorrowStart.toISOString(),
      }),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({sites, matches, revenue}) => {
          const sitesArr = Array.isArray(sites) ? sites : (sites as any)?.items ?? [];
          const activeSites = sitesArr.filter((s: any) => s.isActive).length;

          const matchCount = (matches as any)?.totalCount ?? 0;

          const totalCents = revenue.items.reduce((sum, i) => sum + i.amountCents, 0);
          const caFormatted = (totalCents / 100).toLocaleString('fr-BE', {
            style: 'currency',
            currency: revenue.currency || 'EUR',
            minimumFractionDigits: 2,
          });

          this.kpis.set([
            {label: this.transloco.translate('admin.dashboard.kpis.activeSites'), value: String(activeSites)},
            {label: this.transloco.translate('admin.dashboard.kpis.matchesToday'), value: String(matchCount)},
            {label: this.transloco.translate('admin.dashboard.kpis.revenueToday'), value: caFormatted},
            {label: this.transloco.translate('admin.dashboard.kpis.activeDebts'), value: '—'},
          ]);
        },
        error: () => {
          this.api.getSites().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
            next: (res: any) => {
              const sites = Array.isArray(res) ? res : res?.items ?? [];
              const active = sites.filter((s: any) => s.isActive).length;
              this.kpis.update(k => k.map((card, i) =>
                i === 0 ? {...card, value: String(active)} : card
              ));
            },
          });
        },
      });
  }
}