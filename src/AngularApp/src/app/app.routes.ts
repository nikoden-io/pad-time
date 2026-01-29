import { Routes } from '@angular/router';
import { ShellComponent } from './layout/components';
import { authGuard } from './core/guards';

export const routes: Routes = [
  // Auth routes (outside shell)
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('./features/auth/pages/callback.component').then((m) => m.CallbackComponent),
  },

  // Main app routes (inside shell)
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/booking/pages/booking-home.component').then(
            (m) => m.BookingHomeComponent
          ),
      },
      {
        path: 'booking',
        loadChildren: () =>
          import('./features/booking/booking.routes').then((m) => m.bookingRoutes),
      },
      {
        path: 'matches',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/booking/pages/my-matches.component').then(
            (m) => m.MyMatchesComponent
          ),
      },
      {
        path: 'admin',
        loadChildren: () =>
          import('./features/admin/admin.routes').then((m) => m.adminRoutes),
      },
    ],
  },

  // Fallback
  {
    path: '**',
    redirectTo: '',
  },
];
