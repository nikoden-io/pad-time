import {
  ChangeDetectionStrategy, Component, EventEmitter,
  Input, OnInit, Output,
} from '@angular/core';
import {CommonModule} from '@angular/common';

export type SuccessVariant = 'payment' | 'booking';

const PAYMENT_COLORS = [
  '#4ade80', '#60a5fa', '#a78bfa', '#f97316',
  '#fbbf24', '#f472b6', '#34d399', '#38bdf8',
];
const BOOKING_COLORS = [
  '#fde68a', '#fbbf24', '#86efac', '#4ade80',
  '#93c5fd', '#a5f3fc', '#d9f99d', '#fef08a',
];
const SHAPES = ['square', 'rect', 'circle'];

export interface ConfettiPiece {
  left: string;
  top: string;
  color: string;
  delay: string;
  duration: string;
  width: string;
  height: string;
  shape: string;
  rotate: string;
  drift: string;
  tx: string;
  ty: string;
}

@Component({
  selector: 'app-payment-success-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-success-overlay.component.html',
  styleUrl: './payment-success-overlay.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentSuccessOverlayComponent implements OnInit {
  @Input() variant: SuccessVariant = 'payment';
  @Input() amount = '';
  @Input() title = '';
  @Input() subtitle = '';
  @Output() dismissed = new EventEmitter<void>();

  pieces: ConfettiPiece[] = [];

  get displayTitle() {
    if (this.title) return this.title;
    return this.variant === 'booking' ? 'Réservation confirmée !' : 'Paiement réussi';
  }

  get displaySubtitle() {
    if (this.subtitle) return this.subtitle;
    return this.variant === 'booking' ? 'À vous de jouer ! 🏸' : 'Votre place est confirmée 🎉';
  }

  private rand(min: number, max: number) {
    return min + Math.random() * (max - min);
  }

  ngOnInit(): void {
    const colors = this.variant === 'booking' ? BOOKING_COLORS : PAYMENT_COLORS;

    this.pieces = Array.from({length: 60}, () => {
      const size = this.rand(6, 13);
      const shape = this.variant === 'booking'
        ? 'circle'
        : SHAPES[Math.floor(Math.random() * SHAPES.length)];

      // booking: burst from center; payment: fall from top
      const angle = this.rand(0, 2 * Math.PI);
      const radius = this.rand(120, 340);

      return {
        left:     this.variant === 'booking' ? '50%' : `${this.rand(5, 95)}%`,
        top:      this.variant === 'booking' ? '50%' : '-14px',
        color:    colors[Math.floor(Math.random() * colors.length)],
        delay:    `${this.rand(0, 0.5).toFixed(2)}s`,
        duration: `${this.rand(0.6, 1.1).toFixed(2)}s`,
        width:    shape === 'rect' ? `${size * 0.5}px` : `${size}px`,
        height:   `${size}px`,
        shape,
        rotate: `${Math.floor(this.rand(120, 720))}deg`,
        drift:  `${this.rand(-60, 60).toFixed(0)}px`,
        tx:     `${(Math.cos(angle) * radius).toFixed(0)}px`,
        ty:     `${(Math.sin(angle) * radius).toFixed(0)}px`,
      };
    });

    setTimeout(() => this.dismissed.emit(), this.variant === 'booking' ? 3000 : 3800);
  }
}
