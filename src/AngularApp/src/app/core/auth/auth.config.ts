// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { environment } from '../../../environments/environment';

/**
 * OIDC authentication configuration for the angular-auth-oidc-client library.
 * Configures the identity provider authority, redirect URIs, client credentials,
 * silent token renewal, and secure route prefixes.
 */
export const authConfig: PassedInitialConfig = {
  config: {
    authority: environment.oidc.authority,
    redirectUrl: environment.oidc.redirectUrl,
    postLogoutRedirectUri: environment.oidc.postLogoutRedirectUri,
    clientId: environment.oidc.clientId,
    scope: environment.oidc.scope,
    responseType: environment.oidc.responseType,
    silentRenew: true,
    useRefreshToken: true,
    renewTimeBeforeTokenExpiresInSeconds: 30,
    secureRoutes: [environment.apiUrl],
    logLevel: environment.production ? 0 : 3, // 0=None, 3=Debug
  },
};