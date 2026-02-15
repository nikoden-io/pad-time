import {Component, EventEmitter, Input, Output, OnChanges, SimpleChanges} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {AutoComplete, AutoCompleteCompleteEvent} from 'primeng/autocomplete';
import {Site} from '@core/models';

@Component({
  selector: 'app-site-search',
  standalone: true,
  imports: [FormsModule, AutoComplete],
  templateUrl: './site-search.component.html',
  styleUrls: ['./site-search.component.scss'],
})
export class SiteSearchComponent implements OnChanges {
  @Input({required: true}) sites: Site[] = [];
  @Input() placeholder = 'Search a site';

  @Output() queryChange = new EventEmitter<string>();
  @Output() selectSite = new EventEmitter<Site>();

  query = '';
  filtered: Site[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['sites'] && this.sites?.length > 0) {
      this.filtered = [...this.sites];
    }
  }

  onComplete(event: AutoCompleteCompleteEvent): void {
    const q = (event.query ?? '').toLowerCase().trim();
    this.queryChange.emit(q);
    this.filtered = q ? this.sites.filter((s) => s.name.toLowerCase().includes(q)) : [...this.sites];
  }

  onSelect(site: Site): void {
    this.selectSite.emit(site);
  }

  onClear(): void {
    this.queryChange.emit('');
    this.filtered = [...this.sites];
  }
}
