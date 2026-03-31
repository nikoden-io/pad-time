// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import {Component} from '@angular/core';
import {BookPageComponent} from '@features/booking/components/book-page/book-page.component';
import {PageShellComponent} from '@shared/components/page-shell/page-shell.component';

@Component({
  selector: 'app-booking-home',
  standalone: true,
  imports: [BookPageComponent, PageShellComponent],
  template: `
    <app-page-shell
      eyebrow="Réservation"
      title="Réservez votre"
      titleEm="prochain match"
      subtitle="Choisissez un site, un créneau et confirmez."
      dividerLabel="disponibilités en temps réel">
      <app-book-page/>
    </app-page-shell>
  `
})
export class BookingHomeComponent {
}