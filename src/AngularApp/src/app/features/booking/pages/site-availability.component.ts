import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../../core/services';
import { Site, Court, TimeSlot } from '../../../core/models';
import { CreateMatchModalComponent, CreateMatchData } from '../components/create-match-modal.component';

@Component({
  selector: 'app-site-availability',
  standalone: true,
  imports: [DatePipe, CreateMatchModalComponent],
  template: `
    <div class="availability-page">
      <div class="page-header">
        <h1>{{ site()?.name ?? 'Loading...' }}</h1>
        <p class="subtitle">Select a court and time slot to book</p>
      </div>

      <div class="date-selector">
        <button class="btn btn-icon" (click)="previousDay()" [disabled]="!canGoPrevious()">
          &larr;
        </button>
        <input
          type="date"
          [value]="selectedDate()"
          [min]="todayDate"
          (change)="onDateChange($event)"
          class="date-input"
        />
        <button class="btn btn-icon" (click)="nextDay()">
          &rarr;
        </button>
      </div>

      @if (loading()) {
        <div class="loading-state">
          <span class="spinner"></span>
          <p>Loading availability...</p>
        </div>
      } @else {
        <div class="courts-container">
          @for (court of courts(); track court.courtId) {
            <div class="court-section">
              <h2>{{ court.label }}</h2>
              <div class="slots-grid">
                @for (slot of getSlots(court.courtId); track slot.startAt) {
                  <button
                    class="slot-btn"
                    [class.available]="slot.available"
                    [class.unavailable]="!slot.available"
                    [disabled]="!slot.available"
                    (click)="openCreateModal(court, slot)"
                  >
                    <span class="slot-time">{{ slot.startAt | date:'HH:mm' }}</span>
                    <span class="slot-end">{{ slot.endAt | date:'HH:mm' }}</span>
                    @if (slot.available) {
                      <span class="slot-status">Available</span>
                    } @else {
                      <span class="slot-status">Booked</span>
                    }
                  </button>
                } @empty {
                  <p class="no-slots">No slots available for this date</p>
                }
              </div>
            </div>
          } @empty {
            <div class="empty-state">
              <p>No courts found for this site</p>
            </div>
          }
        </div>
      }
    </div>

    @if (createModalData()) {
      <app-create-match-modal
        [data]="createModalData()!"
        (close)="closeCreateModal()"
        (created)="onMatchCreated($event)"
      />
    }
  `,
  styles: [`
    .availability-page {
      max-width: 1000px;
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
      }
    }

    .date-selector {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 2rem;
      justify-content: center;
    }

    .date-input {
      padding: 0.5rem 1rem;
      border: 1px solid #d1d5db;
      border-radius: 4px;
      font-size: 1rem;
      min-width: 180px;
      text-align: center;

      &:focus {
        outline: none;
        border-color: #4ade80;
      }
    }

    .btn-icon {
      padding: 0.5rem 0.75rem;
      background: #e5e7eb;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;

      &:hover:not(:disabled) {
        background: #d1d5db;
      }

      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }

    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 3rem;
      color: #6b7280;

      .spinner {
        margin-bottom: 1rem;
      }
    }

    .court-section {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      margin-bottom: 1.5rem;

      h2 {
        color: #1a1a2e;
        margin-bottom: 1rem;
        font-size: 1.25rem;
        display: flex;
        align-items: center;
        gap: 0.5rem;

        &::before {
          content: '';
          display: inline-block;
          width: 4px;
          height: 1.25rem;
          background: #4ade80;
          border-radius: 2px;
        }
      }
    }

    .slots-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
      gap: 0.75rem;
    }

    .slot-btn {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 0.75rem;
      border: 2px solid transparent;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s;

      &.available {
        background: #f0fdf4;
        border-color: #86efac;

        &:hover {
          background: #dcfce7;
          border-color: #4ade80;
          transform: translateY(-2px);
        }

        .slot-status {
          color: #059669;
        }
      }

      &.unavailable {
        background: #fef2f2;
        border-color: #fecaca;
        cursor: not-allowed;
        opacity: 0.7;

        .slot-status {
          color: #dc2626;
        }
      }
    }

    .slot-time {
      font-size: 1.125rem;
      font-weight: 600;
      color: #1a1a2e;
    }

    .slot-end {
      font-size: 0.75rem;
      color: #6b7280;
    }

    .slot-status {
      font-size: 0.75rem;
      font-weight: 500;
      margin-top: 0.25rem;
    }

    .no-slots {
      color: #6b7280;
      grid-column: 1 / -1;
      text-align: center;
      padding: 1rem;
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
      background: white;
      border-radius: 8px;
      color: #6b7280;
    }

    .spinner {
      display: inline-block;
      width: 2rem;
      height: 2rem;
      border: 3px solid #e5e7eb;
      border-top-color: #4ade80;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `],
})
export class SiteAvailabilityComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);

  readonly site = signal<Site | null>(null);
  readonly courts = signal<Court[]>([]);
  readonly slotsMap = signal<Map<string, TimeSlot[]>>(new Map());
  readonly loading = signal(true);
  readonly selectedDate = signal(this.getTodayDate());
  readonly createModalData = signal<CreateMatchData | null>(null);

  readonly todayDate = this.getTodayDate();
  private siteId = '';

  ngOnInit(): void {
    this.siteId = this.route.snapshot.params['siteId'];
    this.loadSiteInfo();
    this.loadData();
  }

  private getTodayDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  canGoPrevious(): boolean {
    return this.selectedDate() > this.todayDate;
  }

  previousDay(): void {
    const current = new Date(this.selectedDate());
    current.setDate(current.getDate() - 1);
    this.selectedDate.set(current.toISOString().split('T')[0]);
    this.loadAvailability();
  }

  nextDay(): void {
    const current = new Date(this.selectedDate());
    current.setDate(current.getDate() + 1);
    this.selectedDate.set(current.toISOString().split('T')[0]);
    this.loadAvailability();
  }

  onDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedDate.set(input.value);
    this.loadAvailability();
  }

  private loadSiteInfo(): void {
    this.api.getSites().subscribe({
      next: (sites) => {
        const site = sites.find((s) => s.siteId === this.siteId);
        this.site.set(site ?? null);
      },
    });
  }

  private loadData(): void {
    this.api.getCourts(this.siteId).subscribe({
      next: (courts) => {
        this.courts.set(courts.filter((c) => c.active));
        this.loadAvailability();
      },
      error: (err) => {
        console.error('Failed to load courts', err);
        this.loading.set(false);
      },
    });
  }

  private loadAvailability(): void {
    this.loading.set(true);
    this.api.getAvailability(this.siteId, this.selectedDate()).subscribe({
      next: (response) => {
        // In real app, API would return per-court availability
        // For now, we duplicate for each court
        const map = new Map<string, TimeSlot[]>();
        for (const court of this.courts()) {
          map.set(court.courtId, response.slots);
        }
        this.slotsMap.set(map);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load availability', err);
        this.loading.set(false);
      },
    });
  }

  getSlots(courtId: string): TimeSlot[] {
    return this.slotsMap().get(courtId) ?? [];
  }

  openCreateModal(court: Court, slot: TimeSlot): void {
    this.createModalData.set({
      siteId: this.siteId,
      siteName: this.site()?.name ?? 'Unknown Site',
      courtId: court.courtId,
      courtLabel: court.label,
      slot,
    });
  }

  closeCreateModal(): void {
    this.createModalData.set(null);
  }

  onMatchCreated(matchId: string): void {
    this.createModalData.set(null);
    // Navigate to the match detail page
    this.router.navigate(['/matches', matchId]);
  }
}
