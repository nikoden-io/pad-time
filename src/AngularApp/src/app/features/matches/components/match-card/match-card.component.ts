import {
  ChangeDetectionStrategy, Component, DestroyRef,
  Input, inject, signal,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {ApiService} from '@core/services';
import {Match} from '@core/models';

@Component({
  selector: 'app-match-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './match-card.component.html',
  styleUrls: ['./match-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatchCardComponent {
  @Input() match!: Match;
  @Input() sitesMap: Record<string, string> = {};

  readonly expanded = signal(false);
  readonly cancelling = signal(false);
  readonly paying = signal(false);
  readonly paySuccess = signal(false);
  readonly payError = signal<string | null>(null);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  get siteName() {
    return this.sitesMap[this.match.siteId] ?? this.match.siteId;
  }

  get isPast() {
    return new Date(this.match.startAtUtc) <= new Date();
  }

  get canCancel() {
    return !['locked', 'completed', 'cancelled'].includes(this.match.status) && !this.isPast;
  }

  get canPay() {
    if (this.isPast || ['locked', 'completed', 'cancelled'].includes(this.match.status)) return false;
    if (this.paySuccess()) return false;
    return this.match.participants?.some(p => p.paymentStatus === 'unpaid') ?? false;
  }

  get filledCount() {
    return this.match.participants?.length ?? 0;
  }

  toggle() {
    this.expanded.update(v => !v);
  }

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
          this.paySuccess.set(true);
        },
        error: (err: any) => {
          this.paying.set(false);
          const code = err?.error?.type ?? '';
          if (code === 'payment.idempotency_conflict') {
            this.payError.set('Paiement déjà en cours, réessayez dans un instant.');
          } else {
            this.payError.set(err?.error?.title ?? 'Erreur lors du paiement.');
          }
        },
      });
  }

  format(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }

  formatDate(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', weekday: 'short', day: 'numeric', month: 'short',
    }).format(new Date(utc));
  }

  formatDay(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {timeZone: 'Europe/Brussels', day: 'numeric'}).format(new Date(utc));
  }

  formatMonth(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {timeZone: 'Europe/Brussels', month: 'short'}).format(new Date(utc));
  }

  dots() {
    return Array.from({length: 4}, (_, i) => i < this.filledCount);
  }
}
