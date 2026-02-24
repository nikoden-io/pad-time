import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {RouterLink} from '@angular/router';
import {TranslocoDirective} from '@jsverse/transloco';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';
import {ApiService} from '@core/services';

interface KpiCard {
  label: string;
  value: string;
  delta?: string;
  deltaDown?: boolean;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, TranslocoDirective, PageShellComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardComponent implements OnInit {
  readonly kpis = signal<KpiCard[]>([
    {label: 'Sites actifs', value: '—'},
    {label: "Matches aujourd'hui", value: '—'},
    {label: 'CA du jour', value: '—'},
    {label: 'Dettes actives', value: '—'},
  ]);
  private readonly api = inject(ApiService);

  ngOnInit(): void {
    // Charge les sites pour le KPI count
    this.api.getSites().subscribe({
      next: (res: any) => {
        const sites = Array.isArray(res) ? res : res?.items ?? [];
        const active = sites.filter((s: any) => s.isActive).length;
        this.kpis.update(k => k.map((card, i) =>
          i === 0 ? {...card, value: String(active)} : card
        ));
      },
    });
  }
}
