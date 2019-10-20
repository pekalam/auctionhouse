using Core.Common.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth
{
    public class UsertAuthDbContext : DbContext
    {
        public static bool Seed { get; set; } = false;

        public DbSet<UserAuthenticationData> UserAuth { get; protected set; }

        public UsertAuthDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Seed)
            {
                UserAuthDbContextSeed.Seed(modelBuilder);
            }
            
        }
    }
}
