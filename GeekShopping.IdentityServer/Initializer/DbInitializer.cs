using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer;

public class DbInitializer : IDbInitializer
{
    private readonly MySQLContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly RoleManager<IdentityRole> _role;
    private readonly IConfiguration _configuration;

    public DbInitializer(MySQLContext context,
        UserManager<ApplicationUser> user,
        RoleManager<IdentityRole> role,
        IConfiguration configuration)
    {
        _context = context;
        _user = user;
        _role = role;
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void Initialize()
    {
        if (_role.FindByNameAsync(IdentityConfiguration.Admin).Result != null) return;
        _role.CreateAsync(new IdentityRole(
            IdentityConfiguration.Admin)).GetAwaiter().GetResult();
        _role.CreateAsync(new IdentityRole(
            IdentityConfiguration.Client)).GetAwaiter().GetResult();

        ApplicationUser admin = new ApplicationUser()
        {
            UserName = "rafael-admin",
            Email = "rafael-admin@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+55 (11) 94187-5489",
            FirstName = "Rafael",
            LastName = "Admin"
        };
        var adminPassword = _configuration.GetSection("AdminUser")["AdminPassword"];
        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new ArgumentNullException("Admin password is required");
        }

        _user.CreateAsync(admin, adminPassword).GetAwaiter().GetResult();
        _user.AddToRoleAsync(admin,
            IdentityConfiguration.Admin).GetAwaiter().GetResult();

        var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
        {
            new(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
            new(JwtClaimTypes.GivenName, admin.FirstName),
            new(JwtClaimTypes.FamilyName, admin.LastName),
            new(JwtClaimTypes.Role, IdentityConfiguration.Admin)
        }).Result;

        ApplicationUser client = new ApplicationUser()
        {
            UserName = "rafael-client",
            Email = "rafael-client@gmail.com",
            EmailConfirmed = true,
            PhoneNumber = "+55 (11) 94187-5489",
            FirstName = "Rafael",
            LastName = "Client"
        };
        var clientPassword = _configuration.GetSection("ClientUser")["ClientPassword"];

        if (string.IsNullOrEmpty(clientPassword))
        {
            throw new ArgumentNullException("Client password is required");
        }

        _user.CreateAsync(client, clientPassword).GetAwaiter().GetResult();
        _user.AddToRoleAsync(client,
            IdentityConfiguration.Client).GetAwaiter().GetResult();
        var clientClaims = _user.AddClaimsAsync(client, new Claim[]
        {
            new(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
            new(JwtClaimTypes.GivenName, client.FirstName),
            new(JwtClaimTypes.FamilyName, client.LastName),
            new(JwtClaimTypes.Role, IdentityConfiguration.Client)
        }).Result;
    }
}