import {Component, computed, inject, signal} from '@angular/core';
import {ApiService} from '@core/services';
import {Site} from '@core/models';
import {SiteSearchComponent} from '@features/booking/components/site-search/site-search.component';
import {SitesGridComponent} from '@features/booking/components/sites-grid/sites-grid.component';

@Component({
  selector: 'app-sites-browser',
  standalone: true,
  imports: [SiteSearchComponent, SitesGridComponent],
  templateUrl: './sites-browser.component.html',
  styleUrls: ['./sites-browser.component.scss'],
})
export class SitesBrowserComponent {
  readonly sites = signal<Site[]>([]);
  readonly loading = signal(true);
  readonly selectedSite = signal<Site | null>(null);
  readonly query = signal('');
  readonly filteredSites = computed(() => {
    const all = this.sites();
    const selected = this.selectedSite();

    if (selected) return [selected];

    const q = this.query().toLowerCase().trim();

    return q
      ? all.filter((s) => s.name.toLowerCase().includes(q))
      : all;
  });
  private readonly api = inject(ApiService);

  constructor() {
    this.api.getSites().subscribe({
      next: (sites) => {
        this.sites.set(sites);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onQueryChange(q: string): void {
    this.selectedSite.set(null);
    this.query.set(q);
  }

  onSelectSite(site: Site): void {
    this.selectedSite.set(site);
  }

  onCardSelect(site: Site): void {
    this.selectedSite.set(site);
  }
}
