﻿using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace GeekShopping.IdentityServer.Configuration;

public static class IdentityConfiguration
{
    public const string Admin = "Admin";
    public const string Client = "Client";

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Email(),
            new IdentityResources.Profile()
        };
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("geek_shopping", "GeekShopping Server"),
            new ApiScope(name: "read", "Read data."),
            new ApiScope(name: "write", "Write data."),
            new ApiScope(name: "delete", "Delete data."),
        };

    public static IEnumerable<Client> Clients(IConfiguration configuration) =>
        new List<Client>
         {
            new Client
            {
                ClientId = configuration["IdentityServer:Clients:client:ClientId"],
                    ClientSecrets = { new Secret(configuration["IdentityServer:Clients:client:ClientSecret"].Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = configuration.GetSection("IdentityServer:Clients:client:AllowedScopes").Get<List<string>>()
            },
            new Client
            {
                ClientId = configuration["IdentityServer:Clients:geek_shopping:ClientId"],
                    ClientSecrets = { new Secret(configuration["IdentityServer:Clients:geek_shopping:ClientSecret"].Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = {"https://localhost:4430/signin-oidc"},
                    PostLogoutRedirectUris = {"https://localhost:4430/signout-callback-oidc"},
                    AllowedScopes = configuration.GetSection("IdentityServer:Clients:geek_shopping:AllowedScopes").Get<List<string>>()
            }
         };
}