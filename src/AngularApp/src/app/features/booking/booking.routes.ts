import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards';

export const bookingRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/booking-home.component').then((m) => m.BookingHomeComponent),
  },
  {
    path: ':siteId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/site-availability.component').then((m) => m.SiteAvailabilityComponent),
  },
];
