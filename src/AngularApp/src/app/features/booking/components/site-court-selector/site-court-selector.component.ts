// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
﻿import {
  ChangeDetectionStrategy, Component,
  Input, Output, EventEmitter,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Site} from '@core/models';

@Component({
  selector: 'app-site-court-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './site-court-selector.component.html',
  styleUrls: ['./site-court-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteCourtSelectorComponent {
  @Input() sites: Site[] = [];
  @Input() selectedSiteId: string | null = null;
  @Input() selectedCourtId: string | null = null;

  @Output() siteSelected = new EventEmitter<string>();
  @Output() courtSelected = new EventEmitter<string | null>();

  get selectedSite() {
    return this.sites.find(s => s.siteId === this.selectedSiteId) ?? null;
  }

  selectSite(id: string) {
    if (this.selectedSiteId !== id) this.siteSelected.emit(id);
  }

  selectCourt(id: string) {
    this.courtSelected.emit(this.selectedCourtId === id ? null : id);
  }
}