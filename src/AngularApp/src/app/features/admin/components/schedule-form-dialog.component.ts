import { Component, inject, input, output, signal, OnChanges } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormControl, Validators } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { DatePicker } from 'primeng/datepicker';
import { MultiSelect } from 'primeng/multiselect';
import { ButtonModule } from 'primeng/button';
import { SiteScheduleDto, CreateScheduleRequest } from '@core/models';

const DAY_OPTIONS = [
  { label: 'Mon', value: 1 },
  { label: 'Tue', value: 2 },
  { label: 'Wed', value: 3 },
  { label: 'Thu', value: 4 },
  { label: 'Fri', value: 5 },
  { label: 'Sat', value: 6 },
  { label: 'Sun', value: 0 },
];

@Component({
  selector: 'app-schedule-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    Dialog,
    InputText,
    InputNumber,
    DatePicker,
    MultiSelect,
    ButtonModule,
  ],
  template: `
    <p-dialog
      [header]="schedule() ? 'Edit Schedule' : 'New Schedule'"
      [visible]="visible()"
      (visibleChange)="onVisibleChange($event)"
      [modal]="true"
      [style]="{ width: '95vw', maxWidth: '550px' }"
      [breakpoints]="{ '640px': '100vw' }"
      [draggable]="false"
    >
      <form [formGroup]="form" class="form-grid">
        <div class="form-group">
          <label class="form-label" for="sch-name">Name *</label>
          <input pInputText id="sch-name" formControlName="name" class="w-full" placeholder="e.g. Summer Hours" />
        </div>

        <div class="form-row">
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sch-from">Valid From *</label>
            <p-datepicker
              id="sch-from"
              formControlName="validFrom"
              dateFormat="yy-mm-dd"
              styleClass="w-full"
            />
          </div>
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sch-until">Valid Until</label>
            <p-datepicker
              id="sch-until"
              formControlName="validUntil"
              dateFormat="yy-mm-dd"
              styleClass="w-full"
              [showClear]="true"
            />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sch-open">Opening *</label>
            <p-datepicker
              id="sch-open"
              formControlName="openingTime"
              [timeOnly]="true"
              styleClass="w-full"
            />
          </div>
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sch-close">Closing *</label>
            <p-datepicker
              id="sch-close"
              formControlName="closingTime"
              [timeOnly]="true"
              styleClass="w-full"
            />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="sch-days">Applicable Days</label>
          <p-multiselect
            id="sch-days"
            formControlName="applicableDays"
            [options]="dayOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="All days (leave empty)"
            styleClass="w-full"
            display="chip"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="sch-prio">Priority *</label>
          <p-inputnumber
            id="sch-prio"
            formControlName="priority"
            [min]="0"
            [showButtons]="true"
            styleClass="w-full"
          />
        </div>
      </form>

      <ng-template #footer>
        <div class="dialog-footer">
          <p-button label="Cancel" severity="secondary" (click)="onVisibleChange(false)" />
          <p-button
            [label]="schedule() ? 'Save' : 'Create'"
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
export class ScheduleFormDialogComponent implements OnChanges {
  readonly visible = input.required<boolean>();
  readonly schedule = input<SiteScheduleDto | null>(null);

  readonly visibleChange = output<boolean>();
  readonly saved = output<CreateScheduleRequest>();

  readonly saving = signal(false);

  private readonly fb = inject(FormBuilder);
  readonly dayOptions = DAY_OPTIONS;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    validFrom: [null as Date | null, [Validators.required]],
    validUntil: [null as Date | null],
    openingTime: [null as Date | null, [Validators.required]],
    closingTime: [null as Date | null, [Validators.required]],
    applicableDays: new FormControl<number[] | null>(null),
    priority: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnChanges(): void {
    const s = this.schedule();
    if (s) {
      this.form.patchValue({
        name: s.name,
        validFrom: new Date(s.validFrom),
        validUntil: s.validUntil ? new Date(s.validUntil) : null,
        openingTime: this.parseTime(s.openingTime),
        closingTime: this.parseTime(s.closingTime),
        applicableDays: s.applicableDays,
        priority: s.priority,
      });
    } else {
      this.form.reset({ priority: 0 });
    }
  }

  onVisibleChange(val: boolean): void {
    this.visibleChange.emit(val);
    if (!val) this.form.reset({ priority: 0 });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    const req: CreateScheduleRequest = {
      name: v.name!,
      validFrom: this.formatDate(v.validFrom!),
      validUntil: v.validUntil ? this.formatDate(v.validUntil) : null,
      openingTime: this.formatTime(v.openingTime!),
      closingTime: this.formatTime(v.closingTime!),
      applicableDays: v.applicableDays?.length ? v.applicableDays : null,
      priority: v.priority ?? 0,
    };
    this.saved.emit(req);
  }

  private formatDate(d: Date): string {
    return d.toISOString().split('T')[0];
  }

  private formatTime(d: Date): string {
    return d.toTimeString().slice(0, 5);
  }

  private parseTime(t: string): Date {
    const [h, m] = t.split(':').map(Number);
    const d = new Date();
    d.setHours(h, m, 0, 0);
    return d;
  }
}
