import {
  ApplicationConfig,
  provideZoneChangeDetection,
  isDevMode,
  APP_INITIALIZER,
  provideAppInitializer, inject
} from '@angular/core';
import {provideRouter, withComponentInputBinding} from '@angular/router';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {provideAuth} from 'angular-auth-oidc-client';
import Aura from '@primeuix/themes/aura';
import {providePrimeNG} from "primeng/config";
import {authConfig} from '@core/auth/auth.config';
import {authInterceptor, errorInterceptor} from '@core/interceptors';
import {routes} from './app.routes';
import {provideIcons} from '@ng-icons/core';
import {TranslocoHttpLoader} from './transloco-loader';
import {provideTransloco} from '@jsverse/transloco';
import {LanguageInitService} from '@core/services/language-init.service';

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
    }),
    provideTransloco({
      config: {
        availableLangs: ['en', 'fr', 'nl', 'de'],
        defaultLang: 'en',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader
    }),
    provideAppInitializer(() => {
      const languageInit = inject(LanguageInitService);
      languageInit.initialize();
    })
  ],
};
