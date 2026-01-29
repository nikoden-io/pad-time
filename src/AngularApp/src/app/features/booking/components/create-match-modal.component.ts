import { Component, inject, signal, input, output, computed } from '@angular/core';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../../core/services';
import { AuthService } from '../../../core/auth/auth.service';
import { TimeSlot, MatchType, CreateMatchRequest, getBookingWindowDays } from '../../../core/models';

export interface CreateMatchData {
  siteId: string;
  siteName: string;
  courtId: string;
  courtLabel: string;
  slot: TimeSlot;
}

@Component({
  selector: 'app-create-match-modal',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  template: `
    <div class="modal-overlay" (click)="onOverlayClick($event)">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Create Match</h2>
          <button class="close-btn" (click)="close.emit()">&times;</button>
        </div>

        <div class="modal-body">
          <div class="match-info">
            <p><strong>Site:</strong> {{ data().siteName }}</p>
            <p><strong>Court:</strong> {{ data().courtLabel }}</p>
            <p><strong>Time:</strong> {{ data().slot.startAt | date:'EEEE, MMM d, y HH:mm' }} - {{ data().slot.endAt | date:'HH:mm' }}</p>
          </div>

          @if (bookingWindowError()) {
            <div class="alert alert-warning">
              {{ bookingWindowError() }}
            </div>
          }

          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <div class="form-group">
              <label class="form-label">Match Type</label>
              <div class="radio-group">
                <label class="radio-option">
                  <input type="radio" formControlName="type" value="public" />
                  <span class="radio-label">
                    <strong>Public</strong>
                    <small>Anyone can join and pay</small>
                  </span>
                </label>
                <label class="radio-option">
                  <input type="radio" formControlName="type" value="private" />
                  <span class="radio-label">
                    <strong>Private</strong>
                    <small>Invite specific players</small>
                  </span>
                </label>
              </div>
            </div>

            @if (isPrivate()) {
              <div class="form-group">
                <label class="form-label">Invite Players (optional)</label>
                <p class="form-hint">Add up to 3 players by their matricule</p>

                <div formArrayName="participants" class="participants-list">
                  @for (ctrl of participantsArray.controls; track $index) {
                    <div class="participant-row">
                      <input
                        type="text"
                        [formControlName]="$index"
                        placeholder="e.g., G1234 or S12345"
                        class="form-input"
                      />
                      <button
                        type="button"
                        class="btn btn-icon"
                        (click)="removeParticipant($index)"
                      >
                        &times;
                      </button>
                    </div>
                  }
                </div>

                @if (participantsArray.length < 3) {
                  <button
                    type="button"
                    class="btn btn-secondary btn-sm"
                    (click)="addParticipant()"
                  >
                    + Add Player
                  </button>
                }
              </div>
            }

            <div class="price-info">
              <p>Total price: <strong>60.00 €</strong> (15.00 € per player)</p>
              @if (isPrivate()) {
                <p class="hint">As organizer, you are responsible for filling the match.</p>
              } @else {
                <p class="hint">Players will pay when they join.</p>
              }
            </div>

            @if (error()) {
              <div class="alert alert-error">
                {{ error() }}
              </div>
            }

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" (click)="close.emit()">
                Cancel
              </button>
              <button
                type="submit"
                class="btn btn-primary"
                [disabled]="submitting() || !!bookingWindowError()"
              >
                @if (submitting()) {
                  <span class="spinner"></span> Creating...
                } @else {
                  Create Match
                }
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      border-radius: 8px;
      width: 100%;
      max-width: 500px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.5rem;
      border-bottom: 1px solid #e5e7eb;

      h2 {
        margin: 0;
        color: #1a1a2e;
      }
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 1.5rem;
      cursor: pointer;
      color: #6b7280;
      padding: 0.25rem;
      line-height: 1;

      &:hover {
        color: #1a1a2e;
      }
    }

    .modal-body {
      padding: 1.5rem;
    }

    .match-info {
      background: #f3f4f6;
      padding: 1rem;
      border-radius: 4px;
      margin-bottom: 1.5rem;

      p {
        margin: 0.25rem 0;
        font-size: 0.875rem;
      }
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    .form-label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
      color: #374151;
    }

    .form-hint {
      font-size: 0.875rem;
      color: #6b7280;
      margin-bottom: 0.5rem;
    }

    .radio-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .radio-option {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.75rem;
      border: 1px solid #e5e7eb;
      border-radius: 4px;
      cursor: pointer;
      transition: border-color 0.2s;

      &:hover {
        border-color: #4ade80;
      }

      &:has(input:checked) {
        border-color: #4ade80;
        background: #f0fdf4;
      }

      input {
        margin-top: 0.25rem;
      }
    }

    .radio-label {
      display: flex;
      flex-direction: column;

      strong {
        color: #1a1a2e;
      }

      small {
        color: #6b7280;
        font-size: 0.75rem;
      }
    }

    .participants-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 0.5rem;
    }

    .participant-row {
      display: flex;
      gap: 0.5rem;
    }

    .btn-icon {
      padding: 0.5rem 0.75rem;
      background: #fee2e2;
      color: #991b1b;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;

      &:hover {
        background: #fecaca;
      }
    }

    .btn-sm {
      padding: 0.375rem 0.75rem;
      font-size: 0.875rem;
    }

    .price-info {
      background: #eff6ff;
      padding: 1rem;
      border-radius: 4px;
      margin-bottom: 1.5rem;

      p {
        margin: 0.25rem 0;
      }

      .hint {
        font-size: 0.875rem;
        color: #1e40af;
      }
    }

    .alert {
      padding: 0.75rem 1rem;
      border-radius: 4px;
      margin-bottom: 1rem;
      font-size: 0.875rem;
    }

    .alert-warning {
      background: #fef3c7;
      color: #92400e;
      border: 1px solid #fcd34d;
    }

    .alert-error {
      background: #fee2e2;
      color: #991b1b;
      border: 1px solid #fca5a5;
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.625rem 1.25rem;
      border-radius: 4px;
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: opacity 0.2s;

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }

    .btn-primary {
      background: #4ade80;
      color: #1a1a2e;
    }

    .btn-secondary {
      background: #e5e7eb;
      color: #374151;
    }

    .form-input {
      flex: 1;
      padding: 0.5rem 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 4px;
      font-size: 0.875rem;

      &:focus {
        outline: none;
        border-color: #4ade80;
      }
    }

    .spinner {
      width: 1rem;
      height: 1rem;
      border: 2px solid #1a1a2e;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `],
})
export class CreateMatchModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly data = input.required<CreateMatchData>();
  readonly close = output<void>();
  readonly created = output<string>(); // matchId

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    type: ['public' as MatchType, Validators.required],
    participants: this.fb.array<string>([]),
  });

  readonly isPrivate = computed(() => this.form.get('type')?.value === 'private');

  readonly bookingWindowError = computed(() => {
    const user = this.auth.currentUser();
    if (!user) return 'You must be logged in';

    const slotDate = new Date(this.data().slot.startAt);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const diffDays = Math.ceil((slotDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
    const maxDays = getBookingWindowDays(user.category);

    if (diffDays > maxDays) {
      return `Your membership (${user.category}) allows booking up to ${maxDays} days in advance. This slot is ${diffDays} days away.`;
    }

    return null;
  });

  get participantsArray(): FormArray {
    return this.form.get('participants') as FormArray;
  }

  addParticipant(): void {
    if (this.participantsArray.length < 3) {
      this.participantsArray.push(this.fb.control(''));
    }
  }

  removeParticipant(index: number): void {
    this.participantsArray.removeAt(index);
  }

  onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.close.emit();
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.bookingWindowError()) return;

    this.submitting.set(true);
    this.error.set(null);

    const formValue = this.form.value;
    const participants = (formValue.participants ?? []).filter((p): p is string => !!p?.trim());

    const request: CreateMatchRequest = {
      siteId: this.data().siteId,
      courtId: this.data().courtId,
      startAt: this.data().slot.startAt,
      type: formValue.type as MatchType,
    };

    if (formValue.type === 'private' && participants.length > 0) {
      request.privateParticipantsMatricules = participants;
    }

    this.api.createMatch(request).subscribe({
      next: (response) => {
        this.submitting.set(false);
        this.created.emit(response.matchId);
      },
      error: (err) => {
        this.submitting.set(false);
        const problemDetail = err.error;
        if (problemDetail?.type) {
          switch (problemDetail.type) {
            case 'booking.slot_conflict':
              this.error.set('This slot is no longer available. Someone else booked it.');
              break;
            case 'billing.organizer_debt_block':
              this.error.set('You have an outstanding debt. Please pay it before creating a new match.');
              break;
            case 'booking.reservation_window_denied':
              this.error.set('You cannot book this far in advance with your membership.');
              break;
            case 'booking.site_scope_violation':
              this.error.set('Your membership does not allow booking at this site.');
              break;
            default:
              this.error.set(problemDetail.detail ?? 'An error occurred. Please try again.');
          }
        } else {
          this.error.set('An error occurred. Please try again.');
        }
      },
    });
  }
}
