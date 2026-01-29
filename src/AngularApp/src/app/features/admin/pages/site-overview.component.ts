import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AdminApiService, SiteOverview, Alert } from '../../../core/services';

@Component({
  selector: 'app-site-overview',
  standalone: true,
  template: `
    <div class="site-overview">
      <h1>Site Overview</h1>

      @if (loading()) {
        <p class="loading">Loading...</p>
      } @else if (overview()) {
        <div class="alerts-section">
          <h2>Alerts</h2>
          @if (overview()!.alerts.length === 0) {
            <p class="no-alerts">No alerts</p>
          } @else {
            <ul class="alerts-list">
              @for (alert of overview()!.alerts; track $index) {
                <li class="alert-item" [class]="'alert-' + alert.type">
                  <span class="alert-type">{{ formatAlertType(alert.type) }}</span>
                  <span class="alert-message">{{ alert.message }}</span>
                </li>
              }
            </ul>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .site-overview {
      max-width: 800px;
      margin: 0 auto;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 1.5rem;
    }

    h2 {
      color: #374151;
      font-size: 1.125rem;
      margin-bottom: 1rem;
    }

    .alerts-section {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .alerts-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .alert-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 0.75rem;
      border-radius: 4px;
      margin-bottom: 0.5rem;

      &.alert-j1_incomplete {
        background: #fef3c7;
      }

      &.alert-unpaid {
        background: #fee2e2;
      }

      &.alert-debt {
        background: #fecaca;
      }
    }

    .alert-type {
      font-weight: 600;
      font-size: 0.75rem;
      text-transform: uppercase;
      padding: 0.25rem 0.5rem;
      background: rgba(0, 0, 0, 0.1);
      border-radius: 4px;
    }

    .alert-message {
      color: #374151;
    }

    .no-alerts {
      color: #6b7280;
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }
  `],
})
export class SiteOverviewComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly adminApi = inject(AdminApiService);

  readonly overview = signal<SiteOverview | null>(null);
  readonly loading = signal(true);

  ngOnInit(): void {
    const siteId = this.route.snapshot.params['siteId'];
    this.loadOverview(siteId);
  }

  private loadOverview(siteId: string): void {
    this.adminApi.getSiteOverview(siteId).subscribe({
      next: (data) => {
        this.overview.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load site overview', err);
        this.loading.set(false);
      },
    });
  }

  formatAlertType(type: Alert['type']): string {
    switch (type) {
      case 'j1_incomplete':
        return 'J-1 Incomplete';
      case 'unpaid':
        return 'Unpaid';
      case 'debt':
        return 'Debt';
      default:
        return type;
    }
  }
}
