import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '@core/services';
import { AuthService } from '@core/auth/auth.service';
import { Site, Court, CreateReservationRequest, MatchType } from '@core/models';

@Component({
  selector: 'app-test-reservation',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="test-page">
      <div class="page-header">
        <h1>Test Reservation</h1>
        <p class="subtitle">POST /api/v1/reservations</p>
      </div>

      <div class="user-badge">
        @if (auth.currentUser(); as user) {
          Logged in as <strong>{{ user.matricule }}</strong> ({{ user.category }})
        } @else {
          <span class="warn">Not authenticated</span>
        }
      </div>

      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="reservation-form">
        <!-- Site -->
        <div class="form-group">
          <label class="form-label">Site</label>
          <select formControlName="siteId" class="form-select" (change)="onSiteChange()">
            <option value="">-- Select a site --</option>
            @for (site of sites(); track site.siteId) {
              <option [value]="site.siteId">{{ site.name }}</option>
            }
          </select>
        </div>

        <!-- Court + Add Court -->
        <div class="form-group">
          <label class="form-label">Court</label>
          @if (loadingCourts()) {
            <span class="hint">Loading courts...</span>
          } @else if (form.get('siteId')?.value && courts().length === 0 && !loadingCourts()) {
            <div class="no-courts-banner">
              No courts found for this site. Create one below.
            </div>
          }
          <select formControlName="courtId" class="form-select">
            <option value="">-- Select a court --</option>
            @for (court of courts(); track court.courtId) {
              <option [value]="court.courtId">{{ court.label }}</option>
            }
          </select>

          @if (form.get('siteId')?.value) {
            <div class="add-court-row">
              <input
                type="text"
                class="form-input"
                placeholder="New court label (e.g. Court 1)"
                [formControl]="newCourtLabel"
              />
              <button
                type="button"
                class="btn btn-secondary"
                [disabled]="!newCourtLabel.value?.trim() || addingCourt()"
                (click)="onAddCourt()"
              >
                @if (addingCourt()) {
                  <span class="spinner"></span>
                } @else {
                  + Add Court
                }
              </button>
            </div>
            @if (addCourtError()) {
              <span class="hint hint--error">{{ addCourtError() }}</span>
            }
          }
        </div>

        <!-- StartAt -->
        <div class="form-group">
          <label class="form-label">Start At (your local time)</label>
          <input type="datetime-local" formControlName="startAt" class="form-input" [min]="minDateTime" />
          <span class="hint">Pick a future time in your timezone. It will be converted to UTC before sending to the API.</span>
        </div>

        <!-- Type -->
        <div class="form-group">
          <label class="form-label">Type</label>
          <div class="radio-row">
            <label class="radio-option">
              <input type="radio" formControlName="type" value="public" />
              Public
            </label>
            <label class="radio-option">
              <input type="radio" formControlName="type" value="private" />
              Private
            </label>
          </div>
        </div>

        <!-- Participants (private only) -->
        @if (form.get('type')?.value === 'private') {
          <div class="form-group">
            <label class="form-label">Participant Matricules (max 3)</label>
            <div formArrayName="participants" class="participants-list">
              @for (ctrl of participantsArray.controls; track $index) {
                <div class="participant-row">
                  <input type="text" [formControlName]="$index" placeholder="e.g. G1234" class="form-input" />
                  <button type="button" class="btn btn-danger-sm" (click)="removeParticipant($index)">&times;</button>
                </div>
              }
            </div>
            @if (participantsArray.length < 3) {
              <button type="button" class="btn btn-secondary btn-sm" (click)="addParticipant()">+ Add</button>
            }
          </div>
        }

        <!-- Submit -->
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="submitting() || form.invalid">
            @if (submitting()) {
              <span class="spinner"></span> Creating...
            } @else {
              Create Reservation
            }
          </button>
        </div>
      </form>

      <!-- Result -->
      @if (result()) {
        <div class="result-panel success">
          <h3>Reservation Created</h3>
          <p><strong>Reservation ID:</strong> {{ result()!.reservationId }}</p>
        </div>
      }

      @if (error()) {
        <div class="result-panel error">
          <h3>Error</h3>
          <pre>{{ error() }}</pre>
        </div>
      }
    </div>
  `,
  styles: [`
    .test-page {
      max-width: 600px;
      margin: 0 auto;
    }

    .page-header {
      margin-bottom: 1.5rem;

      h1 {
        color: #1a1a2e;
        margin-bottom: 0.25rem;
      }

      .subtitle {
        color: #6b7280;
        font-family: monospace;
        font-size: 0.875rem;
      }
    }

    .user-badge {
      background: #f3f4f6;
      padding: 0.75rem 1rem;
      border-radius: 4px;
      margin-bottom: 1.5rem;
      font-size: 0.875rem;

      .warn {
        color: #dc2626;
      }
    }

    .reservation-form {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .form-group {
      margin-bottom: 1.25rem;
    }

    .form-label {
      display: block;
      margin-bottom: 0.375rem;
      font-weight: 500;
      color: #374151;
      font-size: 0.875rem;
    }

    .form-input, .form-select {
      width: 100%;
      padding: 0.5rem 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 4px;
      font-size: 0.875rem;
      box-sizing: border-box;

      &:focus {
        outline: none;
        border-color: #4ade80;
      }
    }

    .hint {
      display: block;
      font-size: 0.75rem;
      color: #9ca3af;
      margin-top: 0.25rem;
    }

    .hint--error {
      color: #dc2626;
    }

    .no-courts-banner {
      background: #fef3c7;
      color: #92400e;
      border: 1px solid #fcd34d;
      padding: 0.5rem 0.75rem;
      border-radius: 4px;
      font-size: 0.8rem;
      margin-bottom: 0.5rem;
    }

    .add-court-row {
      display: flex;
      gap: 0.5rem;
      margin-top: 0.5rem;

      .form-input {
        flex: 1;
      }

      .btn {
        white-space: nowrap;
      }
    }

    .radio-row {
      display: flex;
      gap: 1.5rem;
    }

    .radio-option {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
      font-size: 0.875rem;
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

      .form-input {
        flex: 1;
      }
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      font-weight: 500;
      cursor: pointer;
      border: none;
      font-size: 0.875rem;

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }

    .btn-primary {
      background: #4ade80;
      color: #1a1a2e;
      padding: 0.625rem 1.5rem;
    }

    .btn-secondary {
      background: #e5e7eb;
      color: #374151;
    }

    .btn-sm {
      padding: 0.375rem 0.75rem;
      font-size: 0.8rem;
    }

    .btn-danger-sm {
      padding: 0.375rem 0.625rem;
      background: #fee2e2;
      color: #991b1b;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
    }

    .form-actions {
      margin-top: 1.5rem;
    }

    .result-panel {
      margin-top: 1.5rem;
      padding: 1rem 1.25rem;
      border-radius: 8px;

      h3 {
        margin: 0 0 0.5rem;
        font-size: 1rem;
      }

      p {
        margin: 0.25rem 0;
        font-size: 0.875rem;
      }

      pre {
        margin: 0;
        white-space: pre-wrap;
        word-break: break-all;
        font-size: 0.8rem;
      }

      &.success {
        background: #f0fdf4;
        border: 1px solid #86efac;
        color: #166534;
      }

      &.error {
        background: #fef2f2;
        border: 1px solid #fca5a5;
        color: #991b1b;
      }
    }

    .spinner {
      display: inline-block;
      width: 0.875rem;
      height: 0.875rem;
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
export class TestReservationComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  readonly auth = inject(AuthService);

  readonly sites = signal<Site[]>([]);
  readonly courts = signal<Court[]>([]);
  readonly loadingCourts = signal(false);
  readonly addingCourt = signal(false);
  readonly addCourtError = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly result = signal<{ reservationId: string } | null>(null);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    siteId: ['', Validators.required],
    courtId: ['', Validators.required],
    startAt: ['', Validators.required],
    type: ['public' as MatchType, Validators.required],
    participants: this.fb.array<string>([]),
  });

  readonly newCourtLabel = this.fb.control('');

  readonly minDateTime = this.getMinDateTime();

  get participantsArray(): FormArray {
    return this.form.get('participants') as FormArray;
  }

  private getMinDateTime(): string {
    // datetime-local min must be in local time format YYYY-MM-DDTHH:mm
    const now = new Date();
    now.setMinutes(now.getMinutes() + 30); // buffer 30 min ahead
    return now.toISOString().slice(0, 16);
  }

  ngOnInit(): void {
    this.api.getSites().subscribe({
      next: (sites) => this.sites.set(sites),
    });
  }

  onSiteChange(): void {
    const siteId = this.form.get('siteId')?.value;
    this.form.get('courtId')?.setValue('');
    this.courts.set([]);
    this.addCourtError.set(null);

    if (!siteId) return;

    this.loadCourts(siteId);
  }

  private loadCourts(siteId: string): void {
    this.loadingCourts.set(true);
    this.api.getCourts(siteId).subscribe({
      next: (courts) => {
        this.courts.set(courts.filter((c) => c.active));
        this.loadingCourts.set(false);
      },
      error: () => this.loadingCourts.set(false),
    });
  }

  onAddCourt(): void {
    const siteId = this.form.get('siteId')?.value;
    const label = this.newCourtLabel.value?.trim();
    if (!siteId || !label) return;

    this.addingCourt.set(true);
    this.addCourtError.set(null);

    this.api.createCourt(siteId, { label }).subscribe({
      next: (response) => {
        this.addingCourt.set(false);
        this.newCourtLabel.setValue('');
        // Reload courts and auto-select the new one
        this.api.getCourts(siteId).subscribe({
          next: (courts) => {
            const active = courts.filter((c) => c.active);
            this.courts.set(active);
            this.form.get('courtId')?.setValue(response.courtId);
          },
        });
      },
      error: (err) => {
        this.addingCourt.set(false);
        const problem = err.error;
        if (problem?.detail) {
          this.addCourtError.set(problem.detail);
        } else {
          this.addCourtError.set(`HTTP ${err.status}: ${err.message}`);
        }
      },
    });
  }

  addParticipant(): void {
    if (this.participantsArray.length < 3) {
      this.participantsArray.push(this.fb.control(''));
    }
  }

  removeParticipant(index: number): void {
    this.participantsArray.removeAt(index);
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.submitting.set(true);
    this.result.set(null);
    this.error.set(null);

    const v = this.form.value;
    const participants = (v.participants ?? []).filter((p): p is string => !!p?.trim());

    const request: CreateReservationRequest = {
      siteId: v.siteId!,
      courtId: v.courtId!,
      startAt: new Date(v.startAt!).toISOString(),
      type: v.type as MatchType,
    };

    if (v.type === 'private' && participants.length > 0) {
      request.privateParticipantsMatricules = participants;
    }

    this.api.createReservation(request).subscribe({
      next: (response) => {
        this.submitting.set(false);
        this.result.set(response);
      },
      error: (err) => {
        this.submitting.set(false);
        const problem = err.error;
        // Validation errors (400 with errors object)
        if (problem?.errors) {
          const messages = Object.entries(problem.errors as Record<string, string[]>)
            .map(([field, errs]) => `${field}: ${errs.join(', ')}`)
            .join('\n');
          this.error.set(`Validation failed:\n${messages}`);
        } else if (problem?.type) {
          this.error.set(`[${problem.type}] ${problem.detail ?? problem.title ?? 'Unknown error'}`);
        } else {
          this.error.set(`HTTP ${err.status}: ${err.message}`);
        }
      },
    });
  }
}
