// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {
  ChangeDetectionStrategy, Component, DestroyRef,
  EventEmitter, Input, Output, inject, signal,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {ApiService} from '@core/services';
import {Match, Participant} from '@core/models';
import {
  PaymentSuccessOverlayComponent,
} from '@shared/components/payment-success-overlay/payment-success-overlay.component';

@Component({
  selector: 'app-match-card',
  standalone: true,
  imports: [CommonModule, PaymentSuccessOverlayComponent],
  templateUrl: './match-card.component.html',
  styleUrls: ['./match-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatchCardComponent {
  @Input() match!: Match;
  @Input() sitesMap: Record<string, string> = {};
  @Output() paymentDone = new EventEmitter<void>();

  readonly expanded = signal(false);
  readonly cancelling = signal(false);
  readonly paying = signal(false);
  readonly showSuccess = signal(false);
  readonly payError = signal<string | null>(null);
  readonly localParticipants = signal<Participant[] | null>(null);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  get siteName() { return this.sitesMap[this.match.siteId] ?? this.match.siteId; }
  get isPast()   { return new Date(this.match.startAtUtc) <= new Date(); }

  get canCancel() {
    return !['locked', 'completed', 'cancelled'].includes(this.match.status) && !this.isPast;
  }

  get canPay() {
    if (this.isPast || ['locked', 'completed', 'cancelled'].includes(this.match.status)) return false;
    if (this.showSuccess()) return false;
    const parts = this.localParticipants() ?? this.match.participants ?? [];
    return parts.some(p => p.paymentStatus === 'unpaid');
  }

  get displayParticipants(): Participant[] {
    return this.localParticipants() ?? this.match.participants ?? [];
  }

  get paidAmount(): string {
    return ((this.match.priceTotalCents / 4) / 100).toLocaleString('fr-BE', {
      style: 'currency', currency: 'EUR', minimumFractionDigits: 2,
    });
  }

  get filledCount() { return (this.localParticipants() ?? this.match.participants)?.length ?? 0; }

  toggle() { this.expanded.update(v => !v); }

  cancel(e: Event) {
    e.stopPropagation();
    if (!confirm('Annuler ce match ?')) return;
    this.cancelling.set(true);
    this.api.cancelMatch(this.match.matchId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({error: () => this.cancelling.set(false)});
  }

  pay(e: Event) {
    e.stopPropagation();
    this.paying.set(true);
    this.payError.set(null);

    this.api.payMatch(this.match.matchId, crypto.randomUUID())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.paying.set(false);
          // Optimistic: mark first unpaid participant as paid
          const current = (this.localParticipants() ?? this.match.participants ?? []);
          const updated = [...current];
          const idx = updated.findIndex(p => p.paymentStatus === 'unpaid');
          if (idx !== -1) updated[idx] = {...updated[idx], paymentStatus: 'paid'};
          this.localParticipants.set(updated);
          this.showSuccess.set(true);
        },
        error: (err: any) => {
          this.paying.set(false);
          const code = err?.error?.type ?? '';
          this.payError.set(
            code === 'payment.idempotency_conflict'
              ? 'Paiement déjà en cours, réessayez dans un instant.'
              : (err?.error?.title ?? 'Erreur lors du paiement.')
          );
        },
      });
  }

  onSuccessDismissed() {
    this.showSuccess.set(false);
    this.paymentDone.emit();
  }

  format(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }

  formatDay(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {timeZone: 'Europe/Brussels', day: 'numeric'}).format(new Date(utc));
  }

  formatMonth(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {timeZone: 'Europe/Brussels', month: 'short'}).format(new Date(utc));
  }

  dots() { return Array.from({length: 4}, (_, i) => i < this.filledCount); }
}