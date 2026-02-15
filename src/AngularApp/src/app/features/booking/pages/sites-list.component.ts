import {Component, inject, signal, OnInit} from '@angular/core';
import {ApiService} from '@core/services';
import {Site} from '@core/models';

@Component({
  selector: 'app-sites-list',
  standalone: true,
  template: `
    <div class="sites">
      <h1>Sites</h1>

      @if (loading()) {
        <p class="loading">Loading sites...</p>
      } @else {
        <div class="list">
          @for (site of sites(); track site.siteId) {
            <div class="card">
              <div class="name">{{ site.name }}</div>
              <div class="meta">
                <span class="tz">{{ site.timezone }}</span>
              </div>
            </div>
          } @empty {
            <p class="empty">No sites.</p>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .sites {
      max-width: 800px;
      margin: 0 auto;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 1.5rem;
    }

    .list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .card {
      background: white;
      padding: 1rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, .08);
    }

    .name {
      font-weight: 600;
      color: #1a1a2e;
      margin-bottom: .25rem;
    }

    .meta {
      display: flex;
      gap: 1rem;
      color: #6b7280;
      font-size: .875rem;
      flex-wrap: wrap;
    }

    .id {
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
    }

    .loading, .empty {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }
  `],
})
export class SitesListComponent implements OnInit {
  readonly sites = signal<Site[]>([]);
  readonly loading = signal(true);
  private readonly api = inject(ApiService);

  ngOnInit(): void {
    this.api.getSites().subscribe({
      next: (sites) => {
        this.sites.set(sites);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load sites', err);
        this.loading.set(false);
      },
    });
  }
}
