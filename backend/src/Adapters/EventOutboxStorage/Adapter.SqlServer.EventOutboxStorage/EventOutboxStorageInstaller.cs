using Common.Application.Events;
using Common.Application.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.SqlServer.EventOutbox
{
    public static class EventOutboxStorageInstaller
    {
        public static CommonApplicationInstaller AddSqlServerEventOutboxStorageAdapter(this CommonApplicationInstaller installer, IConfiguration? configuration = null,
            string? connectionString = null)
        {
            connectionString ??= configuration!.GetSection(nameof(EventOutboxStorage))["ConnectionString"];
            installer.Services.AddDbContext<EventOutboxDbContext>(opt =>
                opt.UseSqlServer(connectionString)
            );
            installer.Services.AddTransient<IOutboxItemStore, OutboxItemStore>();
            installer.Services.AddTransient<IOutboxItemFinder, OutboxItemFinder>();

            return installer;
        }
    }
}
