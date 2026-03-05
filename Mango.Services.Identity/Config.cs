using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Mango.Services.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("mango", "Mango API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
    new[]
    {
        new ApiResource("mango_api", "Mango API")
        {
            Scopes = { "mango" },
            UserClaims = { "name", "role" }
        }
    };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // service-to-service (machine-to-machine)
            new Client
            {
                ClientId = "mango.m2m",
                ClientName = "Mango Service Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("mango-m2m-secret".Sha256()) },

                AllowedScopes = { "mango" }
            },

            // Mango.Web (interactive user) - Authorization Code + PKCE
            new Client
            {
                ClientId = "mango.web",
                ClientName = "Mango Web",

                // Для server-side MVC нормально иметь секрет
                ClientSecrets = { new Secret("mango-web-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,

                RedirectUris = { "https://localhost:44382/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:44382/signout-callback-oidc" },

                AllowOfflineAccess = true,

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "mango"
                }
            },
        };
}