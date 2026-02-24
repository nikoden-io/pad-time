import {Routes} from '@angular/router';
import {authGuard} from '@core/guards';

export const bookingRoutes: Routes = [
  {
    path: 'matches',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/my-matches.component').then((m) => m.MyMatchesComponent),
  }
];
