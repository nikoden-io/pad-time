import { Component, inject, input, output, signal, OnChanges } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CourtDetailDto } from '@core/models';

@Component({
  selector: 'app-court-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, Dialog, InputText, ButtonModule],
  template: `
    <p-dialog
      [header]="court() ? 'Edit Court' : 'New Court'"
      [visible]="visible()"
      (visibleChange)="onVisibleChange($event)"
      [modal]="true"
      [style]="{ width: '95vw', maxWidth: '400px' }"
      [draggable]="false"
    >
      <form [formGroup]="form">
        <div class="form-group">
          <label class="form-label" for="cf-label">Label *</label>
          <input pInputText id="cf-label" formControlName="label" class="w-full" placeholder="e.g. Court 1" />
          @if (form.controls.label.touched && form.controls.label.errors) {
            <small class="form-error">Required (max 100)</small>
          }
        </div>
      </form>

      <ng-template #footer>
        <div class="dialog-footer">
          <p-button label="Cancel" severity="secondary" (click)="onVisibleChange(false)" />
          <p-button
            [label]="court() ? 'Save' : 'Create'"
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
      .w-full { width: 100%; }
      .dialog-footer { display: flex; justify-content: flex-end; gap: 0.5rem; }
    `,
  ],
})
export class CourtFormDialogComponent implements OnChanges {
  readonly visible = input.required<boolean>();
  readonly court = input<CourtDetailDto | null>(null);

  readonly visibleChange = output<boolean>();
  readonly saved = output<{ label: string }>();

  readonly saving = signal(false);

  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    label: ['', [Validators.required, Validators.maxLength(100)]],
  });

  ngOnChanges(): void {
    const c = this.court();
    if (c) {
      this.form.patchValue({ label: c.label });
    } else {
      this.form.reset();
    }
  }

  onVisibleChange(val: boolean): void {
    this.visibleChange.emit(val);
    if (!val) this.form.reset();
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saved.emit(this.form.getRawValue());
  }
}
