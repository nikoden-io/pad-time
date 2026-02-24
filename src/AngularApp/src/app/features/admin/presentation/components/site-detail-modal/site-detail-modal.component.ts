import {Component, input, output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Dialog} from 'primeng/dialog';
import {ButtonModule} from 'primeng/button';
import {Tag} from 'primeng/tag';
import {Divider} from 'primeng/divider';
import {Tabs, TabList, Tab, TabPanels, TabPanel} from 'primeng/tabs';
import {TranslocoDirective, provideTranslocoScope} from '@jsverse/transloco';
import {SiteDetail} from '@core/models';

@Component({
  selector: 'app-site-detail-modal',
  standalone: true,
  imports: [
    CommonModule,
    Dialog,
    ButtonModule,
    Tag,
    Divider,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('sites')],
  templateUrl: './site-detail-modal.component.html',
  styleUrl: './site-detail-modal.component.scss',
})
export class SiteDetailModalComponent {
  readonly visible = input.required<boolean>();
  readonly site = input.required<SiteDetail | null>();

  readonly visibleChange = output<boolean>();
  readonly edit = output<void>();

  getDayName(day: number): string {
    const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    return days[day] || '';
  }
}
