import {
  ChangeDetectionStrategy, Component, DestroyRef,
  Input, Output, EventEmitter,
  inject, signal,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {ApiService} from '@core/services';
import {Match} from '@core/models';

@Component({
  selector: 'app-public-match-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-match-card.component.html',
  styleUrls: ['./public-match-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicMatchCardComponent {
  @Input() match!: Match;
  @Input() siteName = '';
  @Output() joined = new EventEmitter<void>();

  readonly joining = signal(false);
  readonly joinError = signal<string | null>(null);
  readonly joinSuccess = signal(false);

  private readonly api = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  get paidCount(): number {
    return this.match.participants?.filter(p => p.paymentStatus === 'paid').length ?? 0;
  }

  get availableSeats(): number {
    return 4 - this.paidCount;
  }

  get isFull(): boolean {
    return this.availableSeats <= 0 || this.match.status === 'full';
  }

  get pricePerSeatEur(): string {
    return ((this.match.priceTotalCents / 4) / 100).toFixed(2);
  }

  dots(): boolean[] {
    return Array.from({length: 4}, (_, i) => i < this.paidCount);
  }

  format(utc: string): string {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }

  formatDay(utc: string): string {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', day: 'numeric',
    }).format(new Date(utc));
  }

  formatMonth(utc: string): string {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', month: 'short',
    }).format(new Date(utc));
  }

  join(): void {
    if (this.joining() || this.isFull || this.joinSuccess()) return;

    const idempotencyKey = crypto.randomUUID();
    this.joining.set(true);
    this.joinError.set(null);

    this.api.joinMatch(this.match.matchId, {idempotencyKey})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.joining.set(false);
          this.joinSuccess.set(true);
          this.joined.emit();
        },
        error: (e: any) => {
          this.joining.set(false);
          const code = e?.error?.type ?? '';
          if (code === 'booking.match_full' || code === 'match.full') {
            this.joinError.set('Ce match est complet.');
          } else if (code === 'booking.already_participant') {
            this.joinError.set('Vous participez déjà à ce match.');
          } else {
            this.joinError.set(e?.error?.title ?? 'Une erreur est survenue.');
          }
        },
      });
  }
}
