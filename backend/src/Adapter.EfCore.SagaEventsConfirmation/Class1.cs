using Microsoft.EntityFrameworkCore;
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
        public bool Completed { get; set; }
        public bool Failed { get; set; }
    }

    internal class DbSagaEventToConfirm
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string CorrelationId { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public bool Processed { get; set; }
    }

    internal static class SagaEventsConfirmationAssembler
    {
        public static SagaEventsConfirmation FromDbEntity(DbSagaEventsConfirmation dbEntity, DbSagaEventToConfirm[]? eventsToConfirm = null)
        {
            var unprocessedEvents = eventsToConfirm?.Where(e => !e.Processed).Select(e => e.EventName) ?? Enumerable.Empty<string>();
            var processedEvents = eventsToConfirm?.Where(e => e.Processed).Select(e => e.EventName) ?? Enumerable.Empty<string>();
            return new SagaEventsConfirmation(new CorrelationId(dbEntity.CorrelationId),
                new CommandId(dbEntity.CommandId),
                new HashSet<string>(unprocessedEvents),
                new HashSet<string>(processedEvents), dbEntity.Completed,
                dbEntity.Failed);
        }

        public static DbSagaEventsConfirmation ToDbEntity(long id, SagaEventsConfirmation confirmations)
        {
            return new DbSagaEventsConfirmation
            {
                Id = id,
                CorrelationId = confirmations.CorrelationId.Value,
                CommandId = confirmations.CommandId.Id,
                Completed = confirmations.IsCompleted,
                Failed = confirmations.IsFailed,
            };
        }
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

    internal class EfCoreSagaNotifications : ISagaNotifications, IImmediateNotifications
    {
        private readonly SagaEventsConfirmationDbContext _dbContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EfCoreSagaNotifications(SagaEventsConfirmationDbContext dbContext, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _dbContext = dbContext;
        }

        private Task<DbSagaEventToConfirm> FindEventToConfirm(CorrelationId correlationId, string eventName)
        {
            return _dbContext.SagaEventsToConfirm
                .FirstAsync(e => e.CorrelationId == correlationId.Value && e.EventName == eventName);
        }

        private Task SaveDbConfirmation(long id, SagaEventsConfirmation eventConfirmations)
        {
            var dbEventConfirmations = ToDbEntity(id, eventConfirmations);
            var local = _dbContext.SagaEventsConfirmations.Local.FirstOrDefault(e => e.Id == id);
            if (local == null)
            {
                _dbContext.SagaEventsConfirmations.Update(dbEventConfirmations);
            }
            else
            {
                // instead of clearning tracking
                local.Completed = dbEventConfirmations.Completed;
                local.Failed = dbEventConfirmations.Failed;
            }
            return _dbContext.SaveChangesAsync();
        }

        public async Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var dbEventToConfirm = new DbSagaEventToConfirm
            {
                CorrelationId = correlationId.Value,
                EventName = @event.EventName,
            };
            _dbContext.SagaEventsToConfirm.Add(dbEventToConfirm);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> events) where T : Event
        {
            foreach (var @event in events)
            {
                var dbEventToConfirm = new DbSagaEventToConfirm
                {
                    CorrelationId = correlationId.Value,
                    EventName = @event.EventName,
                };
                _dbContext.SagaEventsToConfirm.Add(dbEventToConfirm);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event
        {
            var eventToConfirm = await FindEventToConfirm(correlationId, @event.EventName);

            eventToConfirm.Processed = true;
            _dbContext.SagaEventsToConfirm.Update(eventToConfirm);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<(SagaEventsConfirmation, DbSagaEventsConfirmation)> FindEventConfirmations(CorrelationId correlationId)
        {
            //Task<DbSagaEventToConfirm[]> eventsToConfirmTask;
            var eventsToConfirm = await _dbContext.SagaEventsToConfirm.Where(e => e.CorrelationId == correlationId.Value)
                .ToArrayAsync();
            

            var eventsConfirmation = await _dbContext.SagaEventsConfirmations
                .FirstAsync(e => e.CorrelationId == correlationId.Value);
            
            //await Task.WhenAll(eventsToConfirmTask, eventsConfirmationTask);

            //var eventsToConfirm = eventsToConfirmTask.Result;
            //var eventsConfirmation = eventsConfirmationTask.Result;

            return (FromDbEntity(eventsConfirmation, eventsToConfirm), eventsConfirmation);
        }

        public async Task MarkSagaAsCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            eventConfirmations.SetCompleted();

            try
            {
                await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations);
            }
            catch (Exception E)
            {

                throw;
            }
        }

        public async Task MarkSagaAsFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var (eventConfirmations, dbEventConfirmations) = await FindEventConfirmations(correlationId);

            eventConfirmations.SetFailed();

            await SaveDbConfirmation(dbEventConfirmations.Id, eventConfirmations);
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

            await SaveDbConfirmation(existing.Id, confirmations);
            
            return;
        }

        public async Task NotifyFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            

            var existing = await _dbContext.SagaEventsConfirmations
                    .FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value);
            if (existing == null) throw new ArgumentException("Could not find event confirmations with correlation id " + correlationId);

            var confirmations = FromDbEntity(existing);
            confirmations.SetFailed();

            await SaveDbConfirmation(existing.Id, confirmations);
            
            return;
        }
    }

    public class EfCoreReadModelNotificaitonsOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string Provider { get; set; } = "sqlite";
    }
}