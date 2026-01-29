import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../../core/services';
import { Match } from '../../../core/models';

@Component({
  selector: 'app-my-matches',
  standalone: true,
  imports: [RouterLink, DatePipe],
  template: `
    <div class="my-matches">
      <h1>My Matches</h1>

      @if (loading()) {
        <p class="loading">Loading your matches...</p>
      } @else {
        <div class="matches-list">
          @for (match of matches(); track match.matchId) {
            <div class="match-card" [class]="'status-' + match.status">
              <div class="match-header">
                <span class="match-type">{{ match.type }}</span>
                <span class="match-status">{{ match.status }}</span>
              </div>
              <div class="match-time">
                {{ match.startAt | date:'EEEE, MMM d, y' }}
                <br />
                {{ match.startAt | date:'HH:mm' }} - {{ match.endAt | date:'HH:mm' }}
              </div>
              <div class="match-participants">
                {{ match.participants.length }}/4 players
              </div>
              <a [routerLink]="['/matches', match.matchId]" class="btn btn-outline">
                View Details
              </a>
            </div>
          } @empty {
            <div class="empty-state">
              <p>You have no matches yet.</p>
              <a routerLink="/booking" class="btn btn-primary">Book a Court</a>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .my-matches {
      max-width: 800px;
      margin: 0 auto;
    }

    h1 {
      color: #1a1a2e;
      margin-bottom: 1.5rem;
    }

    .matches-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .match-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      border-left: 4px solid #d1d5db;

      &.status-public { border-left-color: #4ade80; }
      &.status-private { border-left-color: #fbbf24; }
      &.status-full { border-left-color: #3b82f6; }
      &.status-locked { border-left-color: #8b5cf6; }
      &.status-completed { border-left-color: #6b7280; }
      &.status-cancelled { border-left-color: #ef4444; }
    }

    .match-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.5rem;
    }

    .match-type {
      text-transform: uppercase;
      font-size: 0.75rem;
      font-weight: 600;
      color: #6b7280;
    }

    .match-status {
      text-transform: uppercase;
      font-size: 0.75rem;
      font-weight: 600;
      padding: 0.25rem 0.5rem;
      background: #f3f4f6;
      border-radius: 4px;
    }

    .match-time {
      font-size: 1.125rem;
      color: #1a1a2e;
      margin-bottom: 0.5rem;
    }

    .match-participants {
      color: #6b7280;
      margin-bottom: 1rem;
    }

    .btn {
      display: inline-block;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      text-decoration: none;
      font-weight: 500;
      text-align: center;
    }

    .btn-outline {
      border: 1px solid #4ade80;
      color: #059669;
    }

    .btn-primary {
      background: #4ade80;
      color: #1a1a2e;
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
      background: white;
      border-radius: 8px;

      p {
        color: #6b7280;
        margin-bottom: 1rem;
      }
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }
  `],
})
export class MyMatchesComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly matches = signal<Match[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.loadMatches();
  }

  private loadMatches(): void {
    this.api.getMatches({ scope: 'mine' }).subscribe({
      next: (response) => {
        this.matches.set(response.items);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load matches', err);
        this.loading.set(false);
      },
    });
  }
}
