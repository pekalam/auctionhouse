using Microsoft.EntityFrameworkCore;

namespace Adapter.EfCore.ReadModelNotifications
{
    public class EfCoreReadModelNotificaitonsOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string Provider { get; set; } = "sqlite";
    }

    internal class SagaEventsConfirmationDbContext : DbContext
    {
        public SagaEventsConfirmationDbContext()
        {

        }

        public SagaEventsConfirmationDbContext(DbContextOptions<SagaEventsConfirmationDbContext> options) : base(options)
        {
        }

        public DbSet<DbSagaEventsConfirmation> SagaEventsConfirmations { get; private set; } = null!;
        public DbSet<DbSagaEventToConfirm> SagaEventsToConfirm { get; private set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
#if TEST
            optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Marek\\source\\repos\\Csharp\\auctionhouse\\backend\\src\\Tests\\FunctionalTestsServer.mdf;Integrated Security=True");
#endif
        }
    }
}