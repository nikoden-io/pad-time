import { Routes } from '@angular/router';
import { authGuard, adminGuard } from '../../core/guards';

export const adminRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./pages/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
  },
  {
    path: 'sites/:siteId',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./pages/site-overview.component').then((m) => m.SiteOverviewComponent),
  },
];
