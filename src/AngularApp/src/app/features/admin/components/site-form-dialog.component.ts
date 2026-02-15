import { Component, inject, input, output, signal, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { SiteDetailDto, CreateSiteRequest } from '@core/models';

const COMMON_TIMEZONES = [
  'Europe/Brussels',
  'Europe/Paris',
  'Europe/London',
  'Europe/Berlin',
  'Europe/Amsterdam',
  'Europe/Madrid',
  'Europe/Rome',
  'Europe/Lisbon',
  'Europe/Zurich',
  'Europe/Vienna',
  'America/New_York',
  'America/Chicago',
  'America/Los_Angeles',
  'Asia/Tokyo',
  'Australia/Sydney',
];

@Component({
  selector: 'app-site-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, Dialog, InputText, Select, ButtonModule],
  template: `
    <p-dialog
      [header]="site() ? 'Edit Site' : 'New Site'"
      [visible]="visible()"
      (visibleChange)="onVisibleChange($event)"
      [modal]="true"
      [style]="{ width: '95vw', maxWidth: '550px' }"
      [breakpoints]="{ '640px': '100vw' }"
      [draggable]="false"
    >
      <form [formGroup]="form" class="form-grid">
        <div class="form-group">
          <label class="form-label" for="sf-name">Name *</label>
          <input pInputText id="sf-name" formControlName="name" class="w-full" />
          @if (form.controls.name.touched && form.controls.name.errors) {
            <small class="form-error">Required (max 200)</small>
          }
        </div>

        <div class="form-row">
          <div class="form-group form-group--sm">
            <label class="form-label" for="sf-streetNumber">Number *</label>
            <input pInputText id="sf-streetNumber" formControlName="streetNumber" class="w-full" />
          </div>
          <div class="form-group form-group--lg">
            <label class="form-label" for="sf-street">Street *</label>
            <input pInputText id="sf-street" formControlName="street" class="w-full" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group form-group--sm">
            <label class="form-label" for="sf-postcode">Postcode *</label>
            <input pInputText id="sf-postcode" formControlName="postcode" class="w-full" />
          </div>
          <div class="form-group form-group--lg">
            <label class="form-label" for="sf-city">City *</label>
            <input pInputText id="sf-city" formControlName="city" class="w-full" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sf-country">Country *</label>
            <input pInputText id="sf-country" formControlName="country" class="w-full" />
          </div>
          <div class="form-group" style="flex:1">
            <label class="form-label" for="sf-timezone">Timezone *</label>
            <p-select
              id="sf-timezone"
              formControlName="timezone"
              [options]="timezones"
              [editable]="true"
              placeholder="Select timezone"
              styleClass="w-full"
            />
          </div>
        </div>
      </form>

      <ng-template #footer>
        <div class="dialog-footer">
          <p-button label="Cancel" severity="secondary" (click)="onVisibleChange(false)" />
          <p-button
            [label]="site() ? 'Save' : 'Create'"
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
      .form-grid {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
      }
      .form-row {
        display: flex;
        gap: 0.75rem;
      }
      .form-group--sm {
        flex: 0 0 30%;
      }
      .form-group--lg {
        flex: 1;
      }
      .w-full {
        width: 100%;
      }
      .dialog-footer {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
      }
    `,
  ],
})
export class SiteFormDialogComponent implements OnChanges {
  readonly visible = input.required<boolean>();
  readonly site = input<SiteDetailDto | null>(null);

  readonly visibleChange = output<boolean>();
  readonly saved = output<CreateSiteRequest>();

  readonly saving = signal(false);

  private readonly fb = inject(FormBuilder);

  readonly timezones = COMMON_TIMEZONES;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    streetNumber: ['', [Validators.required, Validators.maxLength(15)]],
    street: ['', [Validators.required, Validators.maxLength(200)]],
    postcode: ['', [Validators.required, Validators.maxLength(10)]],
    city: ['', [Validators.required, Validators.maxLength(200)]],
    country: ['', [Validators.required, Validators.maxLength(200)]],
    timezone: ['', [Validators.required]],
  });

  ngOnChanges(): void {
    const s = this.site();
    if (s) {
      this.form.patchValue({
        name: s.name,
        streetNumber: s.streetNumber,
        street: s.street,
        postcode: s.postcode,
        city: s.city,
        country: s.country,
        timezone: s.timezone,
      });
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
