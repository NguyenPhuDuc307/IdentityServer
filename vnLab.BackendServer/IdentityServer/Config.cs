using IdentityServer4;
using IdentityServer4.Models;

namespace vnLab.BackendServer.IdentityServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
        new IdentityResource[]
        {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
        };

        public static IEnumerable<ApiResource> Apis =>
        new ApiResource[]
        {
        new ApiResource("api.vnlab", "vnLab API")
        };

        public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("api.vnlab", "vnLab API")
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
            new Client
            {
                ClientId = "webportal",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                RequirePkce = true,
                AllowOfflineAccess = true,

                // where to redirect to after login
                RedirectUris = { "https://localhost:5001/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "api.vnlab"
                }
             },
            new Client
            {
                ClientId = "swagger",
                ClientName = "Swagger Client",

                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,

                RedirectUris =           { "https://localhost:5000/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { "https://localhost:5000/swagger/oauth2-redirect.html" },
                AllowedCorsOrigins =     { "https://localhost:5000" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api.vnLab"
                }
            },
            new Client
            {
                ClientName = "Angular Admin",
                ClientId = "angular_admin",
                AccessTokenType = AccessTokenType.Reference,
                RequireConsent = false,

                RequireClientSecret = false,
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                AllowAccessTokensViaBrowser = true,
                RedirectUris = new List<string>
                {
                    "https://localhost:4200",
                    "https://localhost:4200/authentication/login-callback",
                    "https://localhost:4200/silent-renew.html"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    "https://localhost:4200/unauthorized",
                    "https://localhost:4200/authentication/logout-callback",
                    "https://localhost:4200"
                },
                AllowedCorsOrigins = new List<string>
                {
                    "https://localhost:4200"
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api.vnlab"
                }
            }
            };
    }
}