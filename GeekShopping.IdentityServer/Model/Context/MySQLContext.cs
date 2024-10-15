using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.IdentityServer.Model.Context;

public class MySQLContext(DbContextOptions<MySQLContext> options) : IdentityDbContext<ApplicationUser>(options) { }