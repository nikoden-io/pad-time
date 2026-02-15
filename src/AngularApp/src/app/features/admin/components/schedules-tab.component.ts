import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { Divider } from 'primeng/divider';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import {
  SiteScheduleDto,
  SiteClosureDto,
  CourtDetailDto,
  CreateScheduleRequest,
  CreateClosureRequest,
} from '@core/models';
import { SitesStore } from '../services/sites-store.service';
import { ScheduleFormDialogComponent } from './schedule-form-dialog.component';
import { ClosureFormDialogComponent } from './closure-form-dialog.component';

const DAY_LABELS: Record<number, string> = {
  0: 'Sun',
  1: 'Mon',
  2: 'Tue',
  3: 'Wed',
  4: 'Thu',
  5: 'Fri',
  6: 'Sat',
};

@Component({
  selector: 'app-schedules-tab',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    Tag,
    Divider,
    ConfirmDialog,
    ScheduleFormDialogComponent,
    ClosureFormDialogComponent,
  ],
  providers: [ConfirmationService],
  template: `
    <!-- ─── Schedules ─── -->
    <div class="tab-header">
      <h3>Schedules</h3>
      <p-button label="Add Schedule" icon="pi pi-plus" size="small" (click)="openCreateSchedule()" />
    </div>

    @if (schedules().length === 0) {
      <div class="empty-state">
        <p class="text-muted">No schedules defined.</p>
      </div>
    } @else {
      <div class="cards-list">
        @for (s of schedules(); track s.scheduleId) {
          <div class="card schedule-card">
            <div class="schedule-card__header">
              <div>
                <span class="font-medium">{{ s.name }}</span>
                <span class="text-sm text-muted ml-2">Priority {{ s.priority }}</span>
              </div>
              <div class="schedule-card__actions">
                <p-button icon="pi pi-pencil" [text]="true" size="small" (click)="openEditSchedule(s)" />
                <p-button icon="pi pi-trash" [text]="true" severity="danger" size="small" (click)="confirmDeleteSchedule(s)" />
              </div>
            </div>
            <div class="schedule-card__body">
              <div class="schedule-meta">
                <span>{{ s.openingTime }} – {{ s.closingTime }}</span>
                <span class="text-muted">|</span>
                <span>{{ s.validFrom }}{{ s.validUntil ? ' → ' + s.validUntil : ' → ongoing' }}</span>
              </div>
              @if (s.applicableDays?.length) {
                <div class="day-chips">
                  @for (d of s.applicableDays; track d) {
                    <span class="badge badge-info">{{ dayLabel(d) }}</span>
                  }
                </div>
              } @else {
                <span class="text-sm text-muted">All days</span>
              }
            </div>
          </div>
        }
      </div>
    }

    <!-- ─── Closures ─── -->
    <p-divider />

    <div class="tab-header">
      <h3>Closures</h3>
      <p-button label="Add Closure" icon="pi pi-plus" size="small" severity="warn" (click)="openCreateClosure()" />
    </div>

    @if (closures().length === 0) {
      <div class="empty-state">
        <p class="text-muted">No closures defined.</p>
      </div>
    } @else {
      <div class="cards-list">
        @for (c of closures(); track c.closureId) {
          <div class="card closure-card">
            <div class="closure-card__header">
              <div class="closure-card__badges">
                <p-tag [value]="c.type" [severity]="closureTypeSeverity(c.type)" />
                <p-tag [value]="c.reason" severity="info" />
              </div>
              <p-button icon="pi pi-trash" [text]="true" severity="danger" size="small" (click)="confirmDeleteClosure(c)" />
            </div>
            <div class="closure-card__body">
              <span>{{ c.startDate }}{{ c.startDate !== c.endDate ? ' → ' + c.endDate : '' }}</span>
              @if (c.modifiedOpeningTime) {
                <span class="text-sm text-muted">
                  Modified hours: {{ c.modifiedOpeningTime }} – {{ c.modifiedClosingTime }}
                </span>
              }
              @if (c.description) {
                <span class="text-sm text-muted">{{ c.description }}</span>
              }
            </div>
          </div>
        }
      </div>
    }

    <app-schedule-form-dialog
      [visible]="scheduleDialogVisible()"
      [schedule]="editingSchedule()"
      (visibleChange)="scheduleDialogVisible.set($event)"
      (saved)="onSaveSchedule($event)"
    />

    <app-closure-form-dialog
      [visible]="closureDialogVisible()"
      [courts]="courts()"
      (visibleChange)="closureDialogVisible.set($event)"
      (saved)="onSaveClosure($event)"
    />

    <p-confirmdialog />
  `,
  styles: [
    `
      .tab-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 1rem;
      }
      .tab-header h3 { margin: 0; font-size: 1.1rem; }
      .cards-list {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .schedule-card__header,
      .closure-card__header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.5rem;
      }
      .schedule-card__actions { display: flex; gap: 0.25rem; }
      .schedule-card__body {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
      }
      .schedule-meta {
        display: flex;
        gap: 0.5rem;
        align-items: center;
        flex-wrap: wrap;
        font-size: 0.875rem;
      }
      .day-chips {
        display: flex;
        gap: 0.25rem;
        flex-wrap: wrap;
      }
      .closure-card__badges {
        display: flex;
        gap: 0.5rem;
      }
      .closure-card__body {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        font-size: 0.875rem;
      }
      .empty-state { text-align: center; padding: 2rem; }
    `,
  ],
})
export class SchedulesTabComponent {
  readonly siteId = input.required<string>();
  readonly schedules = input.required<SiteScheduleDto[]>();
  readonly closures = input.required<SiteClosureDto[]>();
  readonly courts = input<CourtDetailDto[]>([]);

  private readonly store = inject(SitesStore);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  readonly scheduleDialogVisible = signal(false);
  readonly editingSchedule = signal<SiteScheduleDto | null>(null);
  readonly closureDialogVisible = signal(false);

  dayLabel(d: number): string {
    return DAY_LABELS[d] ?? `${d}`;
  }

  closureTypeSeverity(type: string): 'danger' | 'warn' | 'info' {
    switch (type) {
      case 'FullDay':
      case 'Period':
        return 'danger';
      case 'ReducedHours':
        return 'warn';
      default:
        return 'info';
    }
  }

  // ── Schedules ──

  openCreateSchedule(): void {
    this.editingSchedule.set(null);
    this.scheduleDialogVisible.set(true);
  }

  openEditSchedule(s: SiteScheduleDto): void {
    this.editingSchedule.set(s);
    this.scheduleDialogVisible.set(true);
  }

  onSaveSchedule(req: CreateScheduleRequest): void {
    const editing = this.editingSchedule();
    const onSuccess = () => {
      this.scheduleDialogVisible.set(false);
      this.messageService.add({
        severity: 'success',
        summary: editing ? 'Schedule updated' : 'Schedule created',
      });
    };
    const onError = () =>
      this.messageService.add({ severity: 'error', summary: 'Operation failed' });

    if (editing) {
      this.store.updateSchedule(this.siteId(), editing.scheduleId, req).subscribe({ next: onSuccess, error: onError });
    } else {
      this.store.createSchedule(this.siteId(), req).subscribe({ next: onSuccess, error: onError });
    }
  }

  confirmDeleteSchedule(s: SiteScheduleDto): void {
    this.confirmationService.confirm({
      message: `Delete schedule "${s.name}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.store.deleteSchedule(this.siteId(), s.scheduleId).subscribe({
          next: () =>
            this.messageService.add({ severity: 'success', summary: 'Schedule deleted' }),
          error: () =>
            this.messageService.add({ severity: 'error', summary: 'Could not delete' }),
        });
      },
    });
  }

  // ── Closures ──

  openCreateClosure(): void {
    this.closureDialogVisible.set(true);
  }

  onSaveClosure(req: CreateClosureRequest): void {
    this.store.createClosure(this.siteId(), req).subscribe({
      next: () => {
        this.closureDialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Closure added' });
      },
      error: () =>
        this.messageService.add({ severity: 'error', summary: 'Operation failed' }),
    });
  }

  confirmDeleteClosure(c: SiteClosureDto): void {
    this.confirmationService.confirm({
      message: `Remove this ${c.type} closure?`,
      header: 'Confirm Removal',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.store.deleteClosure(this.siteId(), c.closureId).subscribe({
          next: () =>
            this.messageService.add({ severity: 'success', summary: 'Closure removed' }),
          error: () =>
            this.messageService.add({ severity: 'error', summary: 'Could not remove' }),
        });
      },
    });
  }
}
