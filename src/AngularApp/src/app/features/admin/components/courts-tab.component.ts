import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CourtDetailDto } from '@core/models';
import { SitesStore } from '../services/sites-store.service';
import { CourtFormDialogComponent } from './court-form-dialog.component';

@Component({
  selector: 'app-courts-tab',
  standalone: true,
  imports: [CommonModule, ButtonModule, Tag, ConfirmDialog, CourtFormDialogComponent],
  providers: [ConfirmationService],
  template: `
    <div class="tab-header">
      <h3>Courts</h3>
      <p-button label="Add Court" icon="pi pi-plus" size="small" (click)="openCreate()" />
    </div>

    @if (courts().length === 0) {
      <div class="empty-state">
        <p class="text-muted">No courts yet. Add your first court.</p>
      </div>
    } @else {
      <div class="courts-list">
        @for (court of courts(); track court.courtId) {
          <div class="court-card card">
            <div class="court-card__info">
              <span class="font-medium">{{ court.label }}</span>
              <p-tag
                [value]="court.isActive ? 'Active' : 'Inactive'"
                [severity]="court.isActive ? 'success' : 'danger'"
              />
            </div>
            <div class="court-card__actions">
              <p-button icon="pi pi-pencil" [text]="true" size="small" (click)="openEdit(court)" />
              <p-button
                icon="pi pi-trash"
                [text]="true"
                severity="danger"
                size="small"
                (click)="confirmDelete(court)"
              />
            </div>
          </div>
        }
      </div>
    }

    <app-court-form-dialog
      [visible]="dialogVisible()"
      [court]="editingCourt()"
      (visibleChange)="dialogVisible.set($event)"
      (saved)="onSave($event)"
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
      .tab-header h3 {
        margin: 0;
        font-size: 1.1rem;
      }
      .courts-list {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
      }
      .court-card {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.75rem 1rem;
      }
      .court-card__info {
        display: flex;
        align-items: center;
        gap: 0.75rem;
      }
      .court-card__actions {
        display: flex;
        gap: 0.25rem;
      }
      .empty-state {
        text-align: center;
        padding: 2rem;
      }
    `,
  ],
})
export class CourtsTabComponent {
  readonly siteId = input.required<string>();
  readonly courts = input.required<CourtDetailDto[]>();

  private readonly store = inject(SitesStore);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  readonly dialogVisible = signal(false);
  readonly editingCourt = signal<CourtDetailDto | null>(null);

  openCreate(): void {
    this.editingCourt.set(null);
    this.dialogVisible.set(true);
  }

  openEdit(court: CourtDetailDto): void {
    this.editingCourt.set(court);
    this.dialogVisible.set(true);
  }

  onSave(data: { label: string }): void {
    const court = this.editingCourt();
    const onSuccess = () => {
      this.dialogVisible.set(false);
      this.messageService.add({
        severity: 'success',
        summary: court ? 'Court updated' : 'Court created',
      });
    };
    const onError = () =>
      this.messageService.add({ severity: 'error', summary: 'Operation failed' });

    if (court) {
      this.store.updateCourt(this.siteId(), court.courtId, data).subscribe({ next: onSuccess, error: onError });
    } else {
      this.store.createCourt(this.siteId(), data).subscribe({ next: onSuccess, error: onError });
    }
  }

  confirmDelete(court: CourtDetailDto): void {
    this.confirmationService.confirm({
      message: `Delete court "${court.label}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.store.deleteCourt(this.siteId(), court.courtId).subscribe({
          next: () =>
            this.messageService.add({ severity: 'success', summary: 'Court deleted' }),
          error: () =>
            this.messageService.add({ severity: 'error', summary: 'Could not delete court' }),
        });
      },
    });
  }
}
