using Common.Application.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.SqlServer.EventOutbox
{
    public static class EventOutboxStorageInstaller
    {
        public static void AddSqlServerEventOutboxStorage(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<EventOutboxDbContext>(opt =>
                opt.UseSqlServer(connectionString)
            );
            services.AddTransient<IOutboxItemStore, OutboxItemStore>();
            services.AddTransient<IOutboxItemFinder, OutboxItemFinder>();
        }
    }
}
