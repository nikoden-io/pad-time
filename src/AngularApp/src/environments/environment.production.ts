export const environment = {
  production: true,
  apiUrl: '/api/v1',
  oidc: {
    authority: 'https://identity.padtime.example.com',
    clientId: 'padtime-web',
    redirectUrl: 'https://padtime.example.com/callback',
    postLogoutRedirectUri: 'https://padtime.example.com',
    scope: 'openid profile padtime_profile padel_api padel_admin padel_analytics offline_access',
    responseType: 'code',
  },
};
