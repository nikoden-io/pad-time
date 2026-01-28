import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services';
import { Site } from '../../../core/models';

@Component({
  selector: 'app-booking-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="booking-home">
      <h1>Book a Court</h1>
      <p class="subtitle">Select a site to view available slots</p>

      @if (loading()) {
        <p class="loading">Loading sites...</p>
      } @else if (error()) {
        <p class="error">{{ error() }}</p>
      } @else {
        <div class="sites-grid">
          @for (site of sites(); track site.siteId) {
            <a [routerLink]="['/booking', site.siteId]" class="site-card">
              <h2>{{ site.name }}</h2>
              <p class="timezone">{{ site.timezone }}</p>
            </a>
          } @empty {
            <p>No sites available</p>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .booking-home {
      max-width: 800px;
      margin: 0 auto;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 0.5rem;
    }

    .subtitle {
      color: #6b7280;
      margin-bottom: 2rem;
    }

    .sites-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 1rem;
    }

    .site-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      text-decoration: none;
      color: inherit;
      transition: transform 0.2s, box-shadow 0.2s;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
      }

      h2 {
        color: #1a1a2e;
        margin-bottom: 0.5rem;
      }

      .timezone {
        color: #6b7280;
        font-size: 0.875rem;
      }
    }

    .loading, .error {
      text-align: center;
      padding: 2rem;
    }

    .error {
      color: #ef4444;
    }
  `],
})
export class BookingHomeComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly sites = signal<Site[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadSites();
  }

  private loadSites(): void {
    this.api.getSites().subscribe({
      next: (sites) => {
        this.sites.set(sites);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load sites');
        this.loading.set(false);
        console.error(err);
      },
    });
  }
}
