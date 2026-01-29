import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ApiService } from '../../../core/services';
import { Site } from '../../../core/models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="admin-dashboard">
      <h1>Admin Dashboard</h1>
      <p class="subtitle">
        @if (auth.isGlobalAdmin()) {
          Global Administrator
        } @else {
          Site Administrator
        }
      </p>

      <div class="dashboard-grid">
        <div class="card">
          <h2>Sites</h2>
          @if (loading()) {
            <p>Loading...</p>
          } @else {
            <ul class="site-list">
              @for (site of sites(); track site.siteId) {
                <li>
                  <a [routerLink]="['/admin/sites', site.siteId]">
                    {{ site.name }}
                  </a>
                </li>
              }
            </ul>
          }
        </div>

        <div class="card">
          <h2>Quick Actions</h2>
          <div class="actions">
            <a routerLink="/admin/analytics" class="action-btn">
              View Analytics
            </a>
            <a routerLink="/admin/matches" class="action-btn">
              Manage Matches
            </a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .admin-dashboard {
      max-width: 1000px;
      margin: 0 auto;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 0.25rem;
    }

    .subtitle {
      color: #6b7280;
      margin-bottom: 2rem;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
    }

    .card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

      h2 {
        color: #1a1a2e;
        margin-bottom: 1rem;
        font-size: 1.25rem;
      }
    }

    .site-list {
      list-style: none;
      padding: 0;
      margin: 0;

      li {
        padding: 0.5rem 0;
        border-bottom: 1px solid #f3f4f6;

        &:last-child {
          border-bottom: none;
        }
      }

      a {
        color: #3b82f6;
        text-decoration: none;

        &:hover {
          text-decoration: underline;
        }
      }
    }

    .actions {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .action-btn {
      display: block;
      padding: 0.75rem 1rem;
      background: #f3f4f6;
      border-radius: 4px;
      text-decoration: none;
      color: #1a1a2e;
      transition: background-color 0.2s;

      &:hover {
        background: #e5e7eb;
      }
    }
  `],
})
export class AdminDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  readonly sites = signal<Site[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.loadSites();
  }

  private loadSites(): void {
    this.api.getSites().subscribe({
      next: (sites) => {
        // Filter sites based on user's site_id if site admin
        const user = this.auth.currentUser();
        if (user?.role === 'admin_site' && user.siteId) {
          this.sites.set(sites.filter((s) => s.siteId === user.siteId));
        } else {
          this.sites.set(sites);
        }
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load sites', err);
        this.loading.set(false);
      },
    });
  }
}
