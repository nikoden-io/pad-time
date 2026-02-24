import {Component, computed, effect, inject, input, output, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {Dialog} from 'primeng/dialog';
import {InputText} from 'primeng/inputtext';
import {Select} from 'primeng/select';
import {ButtonModule} from 'primeng/button';
import {TranslocoDirective} from '@jsverse/transloco';
import {provideTranslocoScope} from '@jsverse/transloco';
import {CreateSiteRequest, Site} from '@core/models';

const TIMEZONES = [
  'Europe/Brussels',
  'Europe/Paris',
  'Europe/Amsterdam',
  'Europe/Berlin',
  'Europe/London',
  'America/New_York',
  'America/Los_Angeles',
  'Asia/Tokyo',
];

@Component({
  selector: 'app-site-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Dialog,
    InputText,
    Select,
    ButtonModule,
    TranslocoDirective,
  ],
  providers: [provideTranslocoScope('sites')],
  templateUrl: './site-form-dialog.component.html',
  styleUrl: './site-form-dialog.component.scss',
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
      const currentSite = this.site();
      if (currentSite) {
        this.form.patchValue({
          name: currentSite.name,
          streetNumber: currentSite.streetNumber,
          street: currentSite.street,
          postcode: currentSite.postcode,
          city: currentSite.city,
          country: currentSite.country,
          timezone: currentSite.timezone,
        });
      } else {
        this.form.reset({
          timezone: 'Europe/Brussels',
        });
      }
    });
  }

  onVisibleChange(visible: boolean): void {
    this.visibleChange.emit(visible);
    if (!visible) {
      this.form.reset({timezone: 'Europe/Brussels'});
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    const request: CreateSiteRequest = {
      name: this.form.value.name,
      streetNumber: this.form.value.streetNumber,
      street: this.form.value.street,
      postcode: this.form.value.postcode,
      city: this.form.value.city,
      country: this.form.value.country,
      timezone: this.form.value.timezone,
    };

    this.saved.emit(request);
  }
}
