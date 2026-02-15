import {Component} from '@angular/core';
import {SitesBrowserComponent} from '@features/booking/components/sites-browser/sites-browser.component';

@Component({
  selector: 'app-booking-home',
  standalone: true,
  imports: [SitesBrowserComponent],
  template: `
    <div class="booking-home">
      <h1>Book a Court</h1>
      <p class="subtitle">Select a site to view available slots</p>

      <app-sites-browser></app-sites-browser>
    </div>
  `,
  styles: [
    `
      .booking-home {
        max-width: 800px;
        margin: 0 auto;
        padding: 16px;
      }

      h1 {
        color: #1a1a2e;
        margin-bottom: 0.5rem;
      }

      .subtitle {
        color: #6b7280;
        margin-bottom: 1rem;
      }
    `,
  ],
})
export class BookingHomeComponent {
}
