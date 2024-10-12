using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Initializer.Interface;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext _mySQLContext;
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;
        private readonly IConfiguration _configuration;

        public DbInitializer(MySQLContext mySQLContext, 
            UserManager<ApplicationUser> user, 
            RoleManager<IdentityRole> role,
            IConfiguration configuration)
        {
            _mySQLContext = mySQLContext ?? throw new ArgumentNullException(nameof(mySQLContext));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _role = role ?? throw new ArgumentNullException(nameof(role));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Initialize()
        {
            if (_role.FindByNameAsync(IdentityConfiguration.AdminRole).Result != null) return;

            _role.CreateAsync(new IdentityRole(IdentityConfiguration.AdminRole))
                .GetAwaiter()
                .GetResult();

            _role.CreateAsync(new IdentityRole(IdentityConfiguration.ClientRole))
                .GetAwaiter()
                .GetResult();

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

            _user.CreateAsync(admin, adminPassword)
                .GetAwaiter()
                .GetResult();

            _user.AddToRoleAsync(admin, IdentityConfiguration.AdminRole)
                .GetAwaiter()
                .GetResult();

            var adminClaims = _user.AddClaimsAsync(admin, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.AdminRole),
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

            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentNullException("Client password is required");
            }

            _user.CreateAsync(client, adminPassword)
                .GetAwaiter()
                .GetResult();

            _user.AddToRoleAsync(client, IdentityConfiguration.ClientRole)
                .GetAwaiter()
                .GetResult();

            var clientClaims = _user.AddClaimsAsync(client, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                new Claim(JwtClaimTypes.GivenName, client.FirstName),
                new Claim(JwtClaimTypes.FamilyName, client.LastName),
                new Claim(JwtClaimTypes.Role, IdentityConfiguration.ClientRole),
            }).Result;
        }
    }
}
