// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {
  ChangeDetectionStrategy, Component, DestroyRef,
  computed, inject, signal,
} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';
import {ApiService} from '@core/services';
import {Site, AvailabilitySlot, CreateMatchRequest, SlotSuggestion} from '@core/models';
import {
  SiteCourtSelectorComponent,
} from '@features/booking/components/site-court-selector/site-court-selector.component';
import {SlotPickerComponent} from '@features/booking/components/slot-picker/slot-picker.component';
import {MatchFormComponent, MatchFormOutput} from '@features/booking/components/match-form/match-form.component';
import {
  PaymentSuccessOverlayComponent,
} from '@shared/components/payment-success-overlay/payment-success-overlay.component';
import {
  SmartSuggestionsComponent,
} from '@features/booking/components/smart-suggestions/smart-suggestions.component';

@Component({
  selector: 'app-book-page',
  standalone: true,
  imports: [
    CommonModule,
    SiteCourtSelectorComponent,
    SlotPickerComponent,
    MatchFormComponent,
    PaymentSuccessOverlayComponent,
    SmartSuggestionsComponent,
  ],
  templateUrl: './book-page.component.html',
  styleUrls: ['./book-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookPageComponent {
  // ── State ──────────────────────────────────────────
  readonly sites = signal<Site[]>([]);
  readonly sitesLoading = signal(true);
  readonly selectedSiteId = signal<string | null>(null);
  readonly selectedCourtId = signal<string | null>(null);
  readonly selectedDate = signal<Date>(new Date());
  readonly selectedSlot = signal<AvailabilitySlot | null>(null);
  readonly submitting = signal(false);
  readonly showBookingSuccess = signal(false);
  // ── Derived ────────────────────────────────────────
  readonly selectedSite = computed(() =>
    this.sites().find(s => s.siteId === this.selectedSiteId()) ?? null
  );
  readonly canShowSlots = computed(() => !!this.selectedSiteId());
  readonly canShowForm = computed(() => !!this.selectedSlot());

  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.loadSites();
  }

  // ── Handlers ───────────────────────────────────────
  onSiteSelected(id: string) {
    this.selectedSiteId.set(id);
    this.selectedCourtId.set(null);
    this.selectedSlot.set(null);
  }

  onCourtSelected(id: string | null) {
    this.selectedCourtId.set(id);
    this.selectedSlot.set(null);
  }

  onDateChanged(d: Date) {
    this.selectedDate.set(d);
    this.selectedSlot.set(null);
  }

  onSlotSelected(slot: AvailabilitySlot) {
    this.selectedSlot.set(slot);
    setTimeout(() => document.querySelector('#match-form')?.scrollIntoView({behavior: 'smooth'}), 80);
  }

  onSuggestionSelected(suggestion: SlotSuggestion) {
    this.onSiteSelected(suggestion.siteId);
    this.selectedDate.set(new Date(suggestion.date));
    setTimeout(() => {
      this.selectedCourtId.set(suggestion.courtId);
      this.selectedSlot.set({
        courtId: suggestion.courtId,
        courtLabel: suggestion.courtLabel,
        startAt: suggestion.startAtUtc,
        endAt: suggestion.endAtUtc,
        available: true,
      });
      setTimeout(() => document.querySelector('#match-form')?.scrollIntoView({behavior: 'smooth'}), 80);
    }, 300);
  }

  onCancel() {
    this.selectedSlot.set(null);
  }

  onConfirm(formData: MatchFormOutput) {
    const siteId = this.selectedSiteId();
    const slot = this.selectedSlot();
    if (!siteId || !slot) return;

    const req: CreateMatchRequest = {
      siteId,
      courtId: slot.courtId,
      startAt: slot.startAt,
      type: formData.type,
      ...(formData.type === 'private' ? {privateParticipantsMatricules: formData.participants} : {}),
    };

    this.submitting.set(true);

    this.api.createMatch(req).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.submitting.set(false);
        this.selectedSlot.set(null);
        this.showBookingSuccess.set(true);
      },
      error: (e: any) => {
        this.submitting.set(false);
        // Keep using a simple alert for errors — no toast dependency needed
        console.error('Booking error', e?.error?.title);
      },
    });
  }

  onBookingSuccessDismissed() {
    this.showBookingSuccess.set(false);
    this.router.navigate(['/matches']);
  }

  private loadSites() {
    this.api.getSites().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        this.sites.set(Array.isArray(res) ? res : res?.items ?? []);
        this.sitesLoading.set(false);
      },
      error: () => this.sitesLoading.set(false),
    });
  }
}