using GeekShopping.CartAPI.Controllers.Model;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartAPI.Controllers.Model.Context
{
    public class MySQLContext : DbContext
    {

        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<CartHeader> CartHeaders { get; set; }

        public DbSet<CartDetail> CartDetails { get; set; }
    }
}
