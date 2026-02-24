import {
  ChangeDetectionStrategy, Component,
  Input, Output, EventEmitter,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AvailabilitySlot, Site} from '@core/models';

@Component({
  selector: 'app-match-form',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './match-form.component.html',
  styleUrls: ['./match-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatchFormComponent {
  @Input() slot!: AvailabilitySlot;
  @Input() site: Site | null = null;
  @Input() submitting = false;

  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  format(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels',
      weekday: 'short', day: 'numeric', month: 'short',
      hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }

  formatTime(utc: string) {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }
}
