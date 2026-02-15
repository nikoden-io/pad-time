import {ApplicationConfig, provideZoneChangeDetection} from '@angular/core';
import {provideRouter, withComponentInputBinding} from '@angular/router';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideAuth} from 'angular-auth-oidc-client';
import Aura from '@primeuix/themes/aura';
import {providePrimeNG} from "primeng/config";
import {authConfig} from '@core/auth/auth.config';
import {authInterceptor, errorInterceptor} from '@core/interceptors';
import {routes} from './app.routes';
import {provideIcons} from '@ng-icons/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({eventCoalescing: true}),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    provideAuth(authConfig),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {darkModeSelector: '.p-dark'},
      }
    })
  ],
};
