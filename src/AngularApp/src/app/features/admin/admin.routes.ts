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
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./presentation/pages/sites-list/sites-list.component').then(
        (m) => m.SitesListComponent
      ),
  },
  {
    path: 'overview',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./presentation/pages/admin-overview/admin-overview.component').then(
        (m) => m.AdminOverviewComponent
      ),
  },
  {
    path: 'analytics',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./presentation/pages/admin-analytics/admin-analytics.component').then(
        (m) => m.AdminAnalyticsComponent
      ),
  },
];
