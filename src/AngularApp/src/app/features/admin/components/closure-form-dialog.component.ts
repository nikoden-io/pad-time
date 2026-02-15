import { Component, inject, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { Textarea } from 'primeng/textarea';
import { Select } from 'primeng/select';
import { MultiSelect } from 'primeng/multiselect';
import { DatePicker } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { CourtDetailDto, CreateClosureRequest } from '@core/models';

const CLOSURE_TYPES = [
  { label: 'Full Day', value: 1 },
  { label: 'Period', value: 2 },
  { label: 'Reduced Hours', value: 3 },
];

const CLOSURE_REASONS = [
  { label: 'Public Holiday', value: 1 },
  { label: 'Vacation', value: 2 },
  { label: 'Maintenance', value: 3 },
  { label: 'Private Event', value: 4 },
  { label: 'Weather', value: 5 },
  { label: 'Other', value: 99 },
];

@Component({
  selector: 'app-closure-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Dialog,
    Textarea,
    Select,
    MultiSelect,
    DatePicker,
    ButtonModule,
  ],
  template: `
    <p-dialog
      header="Add Closure"
      [visible]="visible()"
      (visibleChange)="onVisibleChange($event)"
      [modal]="true"
      [style]="{ width: '95vw', maxWidth: '550px' }"
      [breakpoints]="{ '640px': '100vw' }"
      [draggable]="false"
    >
      <form [formGroup]="form" class="form-grid">
        <div class="form-row">
          <div class="form-group" style="flex:1">
            <label class="form-label">Type *</label>
            <p-select
              formControlName="type"
              [options]="closureTypes"
              optionLabel="label"
              optionValue="value"
              placeholder="Select type"
              styleClass="w-full"
            />
          </div>
          <div class="form-group" style="flex:1">
            <label class="form-label">Reason *</label>
            <p-select
              formControlName="reason"
              [options]="closureReasons"
              optionLabel="label"
              optionValue="value"
              placeholder="Select reason"
              styleClass="w-full"
            />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Description</label>
          <textarea pTextarea formControlName="description" class="w-full" rows="2"></textarea>
        </div>

        <div class="form-row">
          <div class="form-group" style="flex:1">
            <label class="form-label">Start Date *</label>
            <p-datepicker formControlName="startDate" dateFormat="yy-mm-dd" styleClass="w-full" />
          </div>
          @if (isPeriod()) {
            <div class="form-group" style="flex:1">
              <label class="form-label">End Date *</label>
              <p-datepicker formControlName="endDate" dateFormat="yy-mm-dd" styleClass="w-full" />
            </div>
          }
        </div>

        @if (isReducedHours()) {
          <div class="form-row">
            <div class="form-group" style="flex:1">
              <label class="form-label">Modified Opening *</label>
              <p-datepicker formControlName="modifiedOpeningTime" [timeOnly]="true" styleClass="w-full" />
            </div>
            <div class="form-group" style="flex:1">
              <label class="form-label">Modified Closing *</label>
              <p-datepicker formControlName="modifiedClosingTime" [timeOnly]="true" styleClass="w-full" />
            </div>
          </div>
        }

        @if (courts().length) {
          <div class="form-group">
            <label class="form-label">Affected Courts</label>
            <p-multiselect
              formControlName="affectedCourtIds"
              [options]="courts()"
              optionLabel="label"
              optionValue="courtId"
              placeholder="All courts (leave empty)"
              styleClass="w-full"
              display="chip"
            />
          </div>
        }
      </form>

      <ng-template #footer>
        <div class="dialog-footer">
          <p-button label="Cancel" severity="secondary" (click)="onVisibleChange(false)" />
          <p-button
            label="Create"
            (click)="onSubmit()"
            [disabled]="form.invalid || saving()"
            [loading]="saving()"
          />
        </div>
      </ng-template>
    </p-dialog>
  `,
  styles: [
    `
      .form-grid { display: flex; flex-direction: column; gap: 0.75rem; }
      .form-row { display: flex; gap: 0.75rem; }
      .w-full { width: 100%; }
      .dialog-footer { display: flex; justify-content: flex-end; gap: 0.5rem; }
      @media (max-width: 639px) {
        .form-row { flex-direction: column; }
      }
    `,
  ],
})
export class ClosureFormDialogComponent {
  readonly visible = input.required<boolean>();
  readonly courts = input<CourtDetailDto[]>([]);

  readonly visibleChange = output<boolean>();
  readonly saved = output<CreateClosureRequest>();

  readonly saving = signal(false);

  readonly closureTypes = CLOSURE_TYPES;
  readonly closureReasons = CLOSURE_REASONS;

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    type: [1 as number, [Validators.required]],
    reason: [null as number | null, [Validators.required]],
    description: [null as string | null],
    startDate: [null as Date | null, [Validators.required]],
    endDate: [null as Date | null],
    modifiedOpeningTime: [null as Date | null],
    modifiedClosingTime: [null as Date | null],
    affectedCourtIds: new FormControl<string[] | null>(null),
  });

  readonly isPeriod = computed(() => this.form.controls.type.value === 2);
  readonly isReducedHours = computed(() => this.form.controls.type.value === 3);

  onVisibleChange(val: boolean): void {
    this.visibleChange.emit(val);
    if (!val) this.form.reset({ type: 1 });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    const req: CreateClosureRequest = {
      type: v.type!,
      reason: v.reason!,
      description: v.description ?? null,
      startDate: this.formatDate(v.startDate!),
      endDate: v.type === 2 && v.endDate ? this.formatDate(v.endDate) : null,
      modifiedOpeningTime:
        v.type === 3 && v.modifiedOpeningTime ? this.formatTime(v.modifiedOpeningTime) : null,
      modifiedClosingTime:
        v.type === 3 && v.modifiedClosingTime ? this.formatTime(v.modifiedClosingTime) : null,
      affectedCourtIds: v.affectedCourtIds?.length ? v.affectedCourtIds : null,
    };
    this.saved.emit(req);
  }

  private formatDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }

  private formatTime(d: Date): string {
    return d.toTimeString().slice(0, 5);
  }
}
