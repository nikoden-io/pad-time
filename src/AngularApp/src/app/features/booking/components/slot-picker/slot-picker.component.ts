import {
  ChangeDetectionStrategy, Component, DestroyRef,
  Input, OnChanges, Output, EventEmitter, inject, signal, computed,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {DatePickerModule} from 'primeng/datepicker';
import {FormsModule} from '@angular/forms';
import {ApiService} from '@core/services';
import {AvailabilitySlot} from '@core/models';

interface CourtGroup {
  courtId: string;
  courtLabel: string;
  slots: AvailabilitySlot[];
}

@Component({
  selector: 'app-slot-picker',
  standalone: true,
  imports: [CommonModule, DatePickerModule, FormsModule],
  templateUrl: './slot-picker.component.html',
  styleUrls: ['./slot-picker.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SlotPickerComponent implements OnChanges {
  @Input() siteId!: string;
  @Input() courtId: string | null = null;
  @Input() selectedDate!: Date;
  @Input() selectedSlot: AvailabilitySlot | null = null;

  @Output() dateChanged = new EventEmitter<Date>();
  @Output() slotSelected = new EventEmitter<AvailabilitySlot>();
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly slots = signal<AvailabilitySlot[]>([]);
  readonly courtGroups = computed<CourtGroup[]>(() => {
    const now = new Date();
    const map = new Map<string, CourtGroup>();
    for (const s of this.slots()) {
      if (new Date(s.startAt) <= now) continue;
      if (!map.has(s.courtId)) map.set(s.courtId, {courtId: s.courtId, courtLabel: s.courtLabel, slots: []});
      map.get(s.courtId)!.slots.push(s);
    }
    return [...map.values()];
  });
  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnChanges(): void {
    if (this.siteId && this.selectedDate) this.load();
  }

  prevDay() {
    this.emit(this.addDays(this.selectedDate, -1));
  }

  nextDay() {
    this.emit(this.addDays(this.selectedDate, 1));
  }

  onDatePick(d: Date | null) {
    if (d) this.emit(d);
  }

  selectSlot(slot: AvailabilitySlot) {
    this.slotSelected.emit(slot);
  }

  format(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }

  isSelected(slot: AvailabilitySlot) {
    return this.selectedSlot?.startAt === slot.startAt && this.selectedSlot?.courtId === slot.courtId;
  }

  private emit(d: Date) {
    this.dateChanged.emit(d);
  }

  private load() {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAvailability({
      siteId: this.siteId,
      date: this.toIso(this.selectedDate),
      ...(this.courtId ? {courtId: this.courtId} : {}),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.slots.set(res.slots ?? []);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les créneaux.');
          this.loading.set(false);
        },
      });
  }

  private toIso(d: Date) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private addDays(d: Date, n: number) {
    const x = new Date(d);
    x.setDate(x.getDate() + n);
    return x;
  }
}
