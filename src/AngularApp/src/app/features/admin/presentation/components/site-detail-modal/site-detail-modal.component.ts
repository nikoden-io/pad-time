// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {ChangeDetectionStrategy, Component, input, output, signal} from '@angular/core';
import {DatePipe} from '@angular/common';
import {Dialog} from 'primeng/dialog';
import {TranslocoDirective, provideTranslocoScope} from '@jsverse/transloco';
import {SiteDetail} from '@core/models';

type Tab = 'courts' | 'schedules' | 'closures';

const DAY_NAMES = ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'];

@Component({
  selector: 'app-site-detail-modal',
  standalone: true,
  imports: [DatePipe, Dialog, TranslocoDirective],
  providers: [provideTranslocoScope('sites')],
  templateUrl: './site-detail-modal.component.html',
  styleUrl: './site-detail-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteDetailModalComponent {
  readonly visible = input.required<boolean>();
  readonly site = input.required<SiteDetail | null>();

  readonly visibleChange = output<boolean>();
  readonly edit = output<void>();

  readonly activeTab = signal<Tab>('courts');

  getDayName(day: number): string {
    return DAY_NAMES[day] ?? '';
  }
}