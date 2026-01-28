export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  oidc: {
    authority: 'https://localhost:5001',
    clientId: 'padtime-web',
    redirectUrl: 'http://localhost:4200/callback',
    postLogoutRedirectUri: 'http://localhost:4200',
    scope: 'openid profile padtime_profile padel_api padel_admin padel_analytics offline_access',
    responseType: 'code',
  },
};
