import { Routes } from '@angular/router';
import { authGuard, adminGuard } from '../../core/guards';
import { SitesStore } from './services/sites-store.service';

export const adminRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard, adminGuard],
    loadComponent: () =>
      import('./pages/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
  },
  {
    path: 'sites',
    canActivate: [authGuard, adminGuard],
    providers: [SitesStore],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/sites-list.component').then((m) => m.SitesListComponent),
      },
      {
        path: ':siteId',
        loadComponent: () =>
          import('./pages/site-detail.component').then((m) => m.SiteDetailComponent),
      },
    ],
  },
];
