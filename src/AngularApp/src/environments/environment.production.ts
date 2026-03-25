export const environment = {
  production: true,
  apiUrl: 'https://backend-api.mangomoss-72bb825b.northeurope.azurecontainerapps.io/api/v1',
  identityApiUrl: 'https://identity-server.mangomoss-72bb825b.northeurope.azurecontainerapps.io/api',
  oidc: {
    authority: 'https://identity-server.mangomoss-72bb825b.northeurope.azurecontainerapps.io',
    clientId: 'padtime-web',
    redirectUrl: 'https://web.mangomoss-72bb825b.northeurope.azurecontainerapps.io/callback',
    postLogoutRedirectUri: 'https://web.mangomoss-72bb825b.northeurope.azurecontainerapps.io',
    scope: 'openid profile padtime_profile padel_api padel_admin padel_analytics offline_access',
    responseType: 'code',
  },
};
