using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new(
            "padtime_profile",
            "Pad'Time Profile",
            [
                CustomClaimTypes.FamilyName,
                CustomClaimTypes.GivenName,
                CustomClaimTypes.Matricule,
                CustomClaimTypes.MemberCategory,
                CustomClaimTypes.SiteId,
                CustomClaimTypes.Role
            ])
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new("padel_api", "Pad'Time API")
        {
            UserClaims =
            [
                CustomClaimTypes.FamilyName,
                CustomClaimTypes.GivenName,
                CustomClaimTypes.Matricule,
                CustomClaimTypes.MemberCategory,
                CustomClaimTypes.SiteId,
                CustomClaimTypes.Role
            ]
        },
        new("padel_admin", "Pad'Time Admin API")
        {
            UserClaims =
            [
                CustomClaimTypes.Role,
                CustomClaimTypes.SiteId
            ]
        },
        new("padel_analytics", "Pad'Time Analytics API")
        {
            UserClaims =
            [
                CustomClaimTypes.Role
            ]
        }
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new("padtime-api", "Pad'Time API")
        {
            Scopes = { "padel_api", "padel_admin", "padel_analytics" },
            UserClaims =
            {
                CustomClaimTypes.FamilyName,
                CustomClaimTypes.GivenName,
                CustomClaimTypes.Matricule,
                CustomClaimTypes.MemberCategory,
                CustomClaimTypes.SiteId,
                CustomClaimTypes.Role
            }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        // m2m client credentials flow client
        new()
        {
            ClientId = "m2m.client",
            ClientName = "Client Credentials Client",

            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

            AllowedScopes = { "scope1" }
        },

        // interactive client using code flow + pkce
        new()
        {
            ClientId = "interactive",
            ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,

            RedirectUris = { "https://localhost:44300/signin-oidc" },
            FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
            PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

            AllowOfflineAccess = true,
            AllowedScopes = { "openid", "profile", "scope2" }
        },

        // Angular SPA client using Authorization Code flow + PKCE
        new()
        {
            ClientId = "padtime-web",
            ClientName = "Pad'Time Web Application",

            // No secret for public SPA client
            RequireClientSecret = false,

            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,

            // Where to redirect after login
            RedirectUris =
            {
                "http://localhost:4200/callback",
                "https://localhost:4200/callback"
            },

            // Where to redirect after logout
            PostLogoutRedirectUris =
            {
                "http://localhost:4200",
                "https://localhost:4200"
            },

            // Allowed CORS origins for token endpoint
            AllowedCorsOrigins =
            {
                "http://localhost:4200",
                "https://localhost:4200"
            },

            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "padtime_profile",
                "padel_api",
                "padel_admin",
                "padel_analytics"
            },

            // Token settings
            AccessTokenLifetime = 3600, // 1 hour
            IdentityTokenLifetime = 3600,

            // Allow refresh tokens
            AllowOfflineAccess = true,
            RefreshTokenUsage = TokenUsage.ReUse,
            RefreshTokenExpiration = TokenExpiration.Sliding,
            SlidingRefreshTokenLifetime = 86400, // 24 hours

            // Always include user claims in ID token
            AlwaysIncludeUserClaimsInIdToken = true
        }
    ];

    private static class CustomClaimTypes
    {
        public const string FamilyName = "family_name";
        public const string GivenName = "given_name";
        public const string Matricule = "matricule";
        public const string MemberCategory = "member_category";
        public const string SiteId = "site_id";
        public const string Role = "role";
    }
}