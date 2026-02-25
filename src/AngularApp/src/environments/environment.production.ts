export const environment = {
  production: true,
  apiUrl: 'https://padtime-api.nikoden.io/api/v1',
  identityApiUrl: 'https://padtime-auth.nikoden.io/api',
  oidc: {
    authority: 'https://padtime-auth.nikoden.io',
    clientId: 'padtime-web',
    redirectUrl: 'https://padtime.nikoden.io/callback',
    postLogoutRedirectUri: 'https://padtime.nikoden.io',
    scope: 'openid profile padtime_profile padel_api padel_admin padel_analytics offline_access',
    responseType: 'code',
  },
};
