import {Component, EventEmitter, Input, Output} from '@angular/core';
import {RouterLink} from '@angular/router';
import {Site} from '@core/models';

@Component({
  selector: 'app-sites-grid',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './sites-grid.component.html',
  styleUrls: ['./sites-grid.component.scss'],
})
export class SitesGridComponent {
  @Input({required: true}) sites: Site[] = [];

  @Input() selectedSiteId: string | null = null;

  // Si null -> pas de navigation, juste sélection
  @Input() linkBase: string | null = null;

  @Input() emptyText = 'No sites.';
  @Output() select = new EventEmitter<Site>();

  onSelect(site: Site, ev: MouseEvent): void {
    if (!this.linkBase) ev.preventDefault();
    this.select.emit(site);
  }

  trackBySiteId(_: number, site: Site): string {
    return site.siteId;
  }
}
