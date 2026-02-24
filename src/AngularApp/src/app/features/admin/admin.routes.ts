import {Routes} from '@angular/router';
import {authGuard, adminGuard} from '@core/guards';

export const adminRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./presentation/pages/admin-dashboard/admin-dashboard.component').then(
        (m) => m.AdminDashboardComponent
      ),
  },
  {
    path: 'sites',
    loadComponent: () =>
      import('./presentation/pages/sites-list/sites-list.component').then(
        (m) => m.SitesListComponent
      ),
  },
];
