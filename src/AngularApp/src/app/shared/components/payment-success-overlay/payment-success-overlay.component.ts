import {
  ChangeDetectionStrategy, Component, EventEmitter,
  Input, OnInit, Output,
} from '@angular/core';
import {CommonModule} from '@angular/common';

const COLORS = [
  '#4ade80', '#60a5fa', '#a78bfa', '#f97316',
  '#fbbf24', '#f472b6', '#34d399', '#38bdf8',
];
const SHAPES = ['square', 'rect', 'circle'];

export interface ConfettiPiece {
  left: string;
  color: string;
  delay: string;
  duration: string;
  width: string;
  height: string;
  shape: string;
  rotate: string;
  drift: string;
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
  @Input() amount = '';
  @Output() dismissed = new EventEmitter<void>();

  pieces: ConfettiPiece[] = [];

  private rand(min: number, max: number) {
    return min + Math.random() * (max - min);
  }

  ngOnInit(): void {
    this.pieces = Array.from({length: 60}, () => {
      const size = this.rand(6, 13);
      const shape = SHAPES[Math.floor(Math.random() * SHAPES.length)];
      return {
        left: `${this.rand(5, 95)}%`,
        color: COLORS[Math.floor(Math.random() * COLORS.length)],
        delay: `${this.rand(0, 0.6).toFixed(2)}s`,
        duration: `${this.rand(0.9, 1.6).toFixed(2)}s`,
        width: shape === 'rect' ? `${size * 0.5}px` : `${size}px`,
        height: `${size}px`,
        shape,
        rotate: `${Math.floor(this.rand(120, 900))}deg`,
        drift: `${this.rand(-60, 60).toFixed(0)}px`,
      };
    });

    setTimeout(() => this.dismissed.emit(), 3800);
  }
}
