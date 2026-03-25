import {
  ChangeDetectionStrategy, Component,
  Input, Output, EventEmitter,
  signal, computed,
} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {AvailabilitySlot, MatchType, Site} from '@core/models';

export interface MatchFormOutput {
  type: MatchType;
  participants: string[];
}

const MATRICULE_RE = /^[GSLgsl]\d{4,5}$/;

@Component({
  selector: 'app-match-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './match-form.component.html',
  styleUrls: ['./match-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MatchFormComponent {
  @Input() slot!: AvailabilitySlot;
  @Input() site: Site | null = null;
  @Input() submitting = false;

  @Output() confirm = new EventEmitter<MatchFormOutput>();
  @Output() cancel = new EventEmitter<void>();

  readonly selectedType = signal<MatchType>('public');
  readonly participants = signal<string[]>([]);
  readonly matriculeInput = signal('');
  readonly matriculeError = signal<string | null>(null);

  readonly canAddParticipant = computed(() => this.participants().length < 3);

  readonly isPrivate = computed(() => this.selectedType() === 'private');

  selectType(type: MatchType) {
    this.selectedType.set(type);
    if (type === 'public') {
      this.participants.set([]);
      this.matriculeError.set(null);
      this.matriculeInput.set('');
    }
  }

  onMatriculeInput(value: string) {
    this.matriculeInput.set(value);
    this.matriculeError.set(null);
  }

  addParticipant() {
    const raw = this.matriculeInput().trim().toUpperCase();

    if (!raw) return;

    if (!MATRICULE_RE.test(raw)) {
      this.matriculeError.set('Format invalide. Exemples : G1234, S01234, L01234');
      return;
    }

    if (this.participants().includes(raw)) {
      this.matriculeError.set('Ce matricule est déjà ajouté.');
      return;
    }

    if (!this.canAddParticipant()) {
      this.matriculeError.set('Maximum 3 participants pour un match privé.');
      return;
    }

    this.participants.update(list => [...list, raw]);
    this.matriculeInput.set('');
    this.matriculeError.set(null);
  }

  removeParticipant(index: number) {
    this.participants.update(list => list.filter((_, i) => i !== index));
  }

  onConfirm() {
    this.confirm.emit({
      type: this.selectedType(),
      participants: this.participants(),
    });
  }

  formatTime(utc: string): string {
    return new Intl.DateTimeFormat('fr-BE', {
      timeZone: 'Europe/Brussels', hour: '2-digit', minute: '2-digit',
    }).format(new Date(utc));
  }
}
