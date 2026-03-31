// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {
  ChangeDetectionStrategy, Component, computed,
  effect, inject, input, output, signal,
} from '@angular/core';
import {ReactiveFormsModule, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Dialog} from 'primeng/dialog';
import {InputText} from 'primeng/inputtext';
import {Select} from 'primeng/select';
import {TranslocoDirective, provideTranslocoScope} from '@jsverse/transloco';
import {CreateSiteRequest, Site} from '@core/models';

const TIMEZONES = [
  'Europe/Brussels', 'Europe/Paris', 'Europe/Amsterdam',
  'Europe/Berlin', 'Europe/London',
  'America/New_York', 'America/Los_Angeles',
  'Asia/Tokyo',
];

@Component({
  selector: 'app-site-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, Dialog, InputText, Select, TranslocoDirective],
  providers: [provideTranslocoScope('sites')],
  templateUrl: './site-form-dialog.component.html',
  styleUrl: './site-form-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteFormDialogComponent {
  readonly visible = input.required<boolean>();
  readonly site = input<Site | null>(null);

  readonly visibleChange = output<boolean>();
  readonly saved = output<CreateSiteRequest>();

  readonly saving = signal(false);
  readonly isEditMode = computed(() => this.site() !== null);
  readonly dialogTitle = computed(() =>
    this.isEditMode() ? 'form.editTitle' : 'form.createTitle'
  );

  readonly timezones = TIMEZONES;

  private readonly fb = inject(FormBuilder);

  readonly form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    streetNumber: ['', Validators.required],
    street: ['', Validators.required],
    postcode: ['', Validators.required],
    city: ['', Validators.required],
    country: ['', Validators.required],
    timezone: ['Europe/Brussels', Validators.required],
  });

  constructor() {
    effect(() => {
      const s = this.site();
      if (s) {
        this.form.patchValue({
          name: s.name, streetNumber: s.streetNumber, street: s.street,
          postcode: s.postcode, city: s.city, country: s.country, timezone: s.timezone,
        });
      } else {
        this.form.reset({timezone: 'Europe/Brussels'});
      }
    });
  }

  onVisibleChange(v: boolean): void {
    this.visibleChange.emit(v);
    if (!v) this.form.reset({timezone: 'Europe/Brussels'});
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const {name, streetNumber, street, postcode, city, country, timezone} = this.form.value;
    this.saved.emit({name, streetNumber, street, postcode, city, country, timezone});
  }
}