using Common.Application.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.SqlServer.EventOutbox
{
    public static class EventOutboxStorageInstaller
    {
        public static void AddSqlServerEventOutboxStorage(this IServiceCollection services, IConfiguration? configuration = null, 
            string? connectionString = null)
        {
            connectionString ??= configuration!.GetSection(nameof(EventOutboxStorage))["ConnectionString"];
            services.AddDbContext<EventOutboxDbContext>(opt =>
                opt.UseSqlServer(connectionString)
            );
            services.AddTransient<IOutboxItemStore, OutboxItemStore>();
            services.AddTransient<IOutboxItemFinder, OutboxItemFinder>();
        }
    }
}
