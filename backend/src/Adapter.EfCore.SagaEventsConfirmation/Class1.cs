using Microsoft.EntityFrameworkCore;
using static Adapter.EfCore.ReadModelNotifications.CollectionFormatter;
using static Adapter.EfCore.ReadModelNotifications.SagaEventsConfirmationAssembler;

namespace Adapter.EfCore.ReadModelNotifications
{
    using Common.Application.SagaNotifications;
    using Common.Application.Events;
    using Common.Application.Commands;
    using System.Threading.Tasks;
    using Core.Common.Domain;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.Extensions.DependencyInjection;

    internal class DbSagaEventsConfirmation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string CommandId { get; set; } = null!;
        public string CorrelationId { get; set; } = null!;
        public string? UnprocessedEvents { get; set; }
        public string? ProcessedEvents { get; set; }
        public bool Completed { get; set; }
    }

    internal static class CollectionFormatter
    {
        private const char Separator = ',';

        public static string CollectionToString(IEnumerable<string> enumerable)
        {
            return string.Join(Separator, enumerable);
        }

        public static IEnumerable<string> StringToCollection(string? str)
        {
            if (str is null) return Enumerable.Empty<string>();

            return str.Split(Separator);
        }
    }

    internal static class SagaEventsConfirmationAssembler
    {
        public static SagaEventsConfirmation FromDbEntity(DbSagaEventsConfirmation dbEntity)
        {
            return new SagaEventsConfirmation(new CorrelationId(dbEntity.CorrelationId),
                new CommandId(dbEntity.CommandId),
                new HashSet<string>(StringToCollection(dbEntity.UnprocessedEvents)),
                new HashSet<string>(StringToCollection(dbEntity.ProcessedEvents)), dbEntity.Completed);
        }

        public static DbSagaEventsConfirmation ToDbEntity(long id, SagaEventsConfirmation confirmations)
        {
            return new DbSagaEventsConfirmation
            {
                Id = id,
                CorrelationId = confirmations.CorrelationId.Value,
                CommandId = confirmations.CommandId.Id,
                ProcessedEvents = CollectionToString(confirmations.ProcessedEvents),
                UnprocessedEvents = CollectionToString(confirmations.UnprocessedEvents),
                Completed = confirmations.IsCompleted,
            };
        }
    }

    internal class SagaEventsConfirmationDbContext : DbContext
    {
        public SagaEventsConfirmationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DbSagaEventsConfirmation> SagaEventsConfirmations { get; private set; } = null!;
    }

    internal class EfCoreSagaNotifications : ISagaNotifications, IImmediateNotifications
    {
        private readonly SagaEventsConfirmationDbContext _dbContext;

        public EfCoreSagaNotifications(SagaEventsConfirmationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private Task<(SagaEventsConfirmation, DbSagaEventsConfirmation)> FindEventConfirmations(CorrelationId correlationId)
        {
            return _dbContext.SagaEventsConfirmations
                .FirstAsync(e => e.CorrelationId == correlationId.Value)
                .ContinueWith(t => (FromDbEntity(t.Result), t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private Task SaveDbConfirmation(long id, SagaEventsConfirmation eventConfirmations, Action<DbSagaEventsConfirmation, DbSagaEventsConfirmation>? updateLocal = null)
        {
            var dbEventConfirmations = ToDbEntity(id, eventConfirmations);
            var local = _dbContext.SagaEventsConfirmations.Local.FirstOrDefault(e => e.Id == id);
            if (local == null)
            {
                _dbContext.SagaEventsConfirmations.Update(dbEventConfirmations);
            }
            else
            {
                if(updateLocal != null) updateLocal(local, dbEventConfirmations);
                else
                {                
                    // instead of clearning tracking
                    local.ProcessedEvents = dbEventConfirmations.ProcessedEvents;
                    local.UnprocessedEvents = dbEventConfirmations.UnprocessedEvents;
                    local.Completed = dbEventConfirmations.Completed;
                }
            }
            return _dbContext.SaveChangesAsync();
        }

        public async Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            if (eventConfirmations.AddUnprocessedEvent(@event.EventName))
            {
                await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations);
            }
        }

        public async Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> events) where T : Event
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            var needsSave = false;
            foreach (var @event in events)
            {
                needsSave = eventConfirmations.AddUnprocessedEvent(@event.EventName) || needsSave;
            }

            if (needsSave)
            {
                await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations);
            }
        }

        public async Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            if (eventConfirmations.MarkEventAsProcessed(@event.EventName))
            {
                await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations);
            }
        }

        public async Task MarkSagaAsCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            eventConfirmations.SetCompleted();

            await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations, (local, db) => local.Completed = true);
        }

        public async Task MarkSagaAsFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            eventConfirmations.SetFailed();

            await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations, (local, db) => local.Completed = false);
        }

        public async Task RegisterNewSaga(CorrelationId correlationId, CommandId commandId)
        {
            var existing = await _dbContext.SagaEventsConfirmations
                .FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value);
            if (existing != null) return;

            var dbEventConfirmations = new DbSagaEventsConfirmation
            {
                CommandId = commandId.Id,
                CorrelationId = correlationId.Value,
            };
            _dbContext.SagaEventsConfirmations.Add(dbEventConfirmations);
            await _dbContext.SaveChangesAsync();
        }

        public async Task RegisterNew(CorrelationId correlationId, CommandId commandId)
        {
            var existing = await _dbContext.SagaEventsConfirmations
                    .FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value);
            if (existing != null) return;

            var dbEventConfirmations = new DbSagaEventsConfirmation
            {
                CommandId = commandId.Id,
                CorrelationId = correlationId.Value,
            };
            _dbContext.SagaEventsConfirmations.Add(dbEventConfirmations);
            await _dbContext.SaveChangesAsync();
        }

        public async Task NotifyCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var existing = await _dbContext.SagaEventsConfirmations
                .FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value);
            if (existing == null) throw new ArgumentException("Could not find event confirmations with correlation id " + correlationId);

            var confirmations = FromDbEntity(existing);
            confirmations.SetCompleted();

            return;
        }

        public async Task NotifyFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var existing = await _dbContext.SagaEventsConfirmations
                    .FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value);
            if (existing == null) throw new ArgumentException("Could not find event confirmations with correlation id " + correlationId);

            var confirmations = FromDbEntity(existing);
            confirmations.SetFailed();

            return;
        }
    }

    public class EfCoreReadModelNotificaitonsOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string Provider { get; set; } = "sqlite";
    }

    public static class EfCoreReadModelNotificationsInstaller
    {
        public static void AddEfCoreReadModelNotifications(this IServiceCollection services, EfCoreReadModelNotificaitonsOptions settings, ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
        {
            services.AddDbContext<SagaEventsConfirmationDbContext>(opt =>
                ConfigureDbContext(settings, opt), contextLifetime);
            services.AddTransient<ISagaNotifications, EfCoreSagaNotifications>();
            services.AddTransient<IImmediateNotifications, EfCoreSagaNotifications>();
        }

        private static DbContextOptionsBuilder ConfigureDbContext(EfCoreReadModelNotificaitonsOptions settings, DbContextOptionsBuilder opt)
        {
            if (settings.Provider.ToLower() == "sqlite")
            {
                return opt.UseSqlite(settings.ConnectionString);
            }
            else if (settings.Provider.ToLower() == "sqlserver")
            {
                return opt.UseSqlServer(settings.ConnectionString);
            }
            else throw new NotImplementedException();
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<SagaEventsConfirmationDbContext>().Database.EnsureCreated();
        }
    }
}