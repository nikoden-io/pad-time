import {Routes} from '@angular/router';
import {authGuard} from '@core/guards';

export const bookingRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/booking-home.component').then((m) => m.BookingHomeComponent),
  },
  {
    path: 'matches',
    canActivate: [authGuard],
    loadComponent: () =>
      import('../matches/pages/my-matches.component').then((m) => m.MyMatchesComponent),
  }
];
