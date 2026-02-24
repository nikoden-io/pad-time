import {
  ChangeDetectionStrategy, Component, DestroyRef,
  computed, inject, signal,
} from '@angular/core';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {CommonModule} from '@angular/common';
import {ToastModule} from 'primeng/toast';
import {MessageService} from 'primeng/api';
import {ApiService} from '@core/services';
import {Site, AvailabilitySlot, CreateMatchRequest} from '@core/models';
import {
  SiteCourtSelectorComponent
} from '@features/booking/components/site-court-selector/site-court-selector.component';
import {SlotPickerComponent} from '@features/booking/components/slot-picker/slot-picker.component';
import {MatchFormComponent} from '@features/booking/components/match-form/match-form.component';

@Component({
  selector: 'app-book-page',
  standalone: true,
  imports: [CommonModule, ToastModule, SiteCourtSelectorComponent, SlotPickerComponent, MatchFormComponent],
  providers: [MessageService],
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
  // ── Derived ────────────────────────────────────────
  readonly selectedSite = computed(() =>
    this.sites().find(s => s.siteId === this.selectedSiteId()) ?? null
  );
  readonly canShowSlots = computed(() => !!this.selectedSiteId());
  readonly canShowForm = computed(() => !!this.selectedSlot());
  private readonly api = inject(ApiService);
  private readonly toast = inject(MessageService);
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

  onCancel() {
    this.selectedSlot.set(null);
  }

  onConfirm() {
    const siteId = this.selectedSiteId();
    const slot = this.selectedSlot();
    if (!siteId || !slot) return;

    const req: CreateMatchRequest = {siteId, courtId: slot.courtId, startAt: slot.startAt, type: 'public'};
    this.submitting.set(true);

    console.log('createMatch payload', req);

    this.api.createMatch(req).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.submitting.set(false);
        this.selectedSlot.set(null);
        this.toast.add({severity: 'success', summary: 'Match créé !', life: 4000});
      },
      error: (e: any) => {
        console.log('error detail', JSON.stringify(e?.error));
        this.submitting.set(false);
        this.toast.add({severity: 'error', summary: e?.error?.title ?? 'Erreur', life: 5000});
      },
    });
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
