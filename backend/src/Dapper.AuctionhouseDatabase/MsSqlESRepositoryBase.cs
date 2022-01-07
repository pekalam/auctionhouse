using Core.Common.Domain;
using Core.DomainFramework;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    internal class MsSqlESRepositoryBase
    {
        private MsSqlConnectionSettings _connectionSettings;

        public MsSqlESRepositoryBase(MsSqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        private Event DeserializeEvent(string evStr)
        {
            try
            {
                Event? ev = JsonConvert.DeserializeObject<Event>(evStr, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                if (ev is null)
                {
                    throw new NullReferenceException($"{nameof(JsonConvert.DeserializeObject)} returned null");
                }
                return ev;
            }
            catch (Exception e)
            {
                throw new InfrastructureException($"Could not DeserializeEvent from {evStr}", e);
            }
        }

        private List<Event> DeserializeEvents(IEnumerable<string> events)
        {
            var aggEvents = new List<Event>();
            foreach (var evStr in events)
            {
                Event ev = DeserializeEvent(evStr);
                aggEvents.Add(ev);
            }

            return aggEvents;
        }

        protected internal virtual List<Event>? ReadEvents(Guid aggId)
        {
            var sql = "SELECT e.Data FROM dbo.Events e WHERE e.AggId = @AggId";
            IEnumerable<string> events;

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                events = connection.Query<string>(sql,
                    new { AggId = aggId });
                if (events.Count() == 0)
                {
                    return null;
                }
            }

            var aggEvents = DeserializeEvents(events);
            return aggEvents;
        }


        protected internal virtual List<Event>? ReadEvents(Guid aggId, long version)
        {
            var sql = "SELECT e.Data FROM dbo.Events e WHERE e.AggId = @AggId AND e.Version <= @Version";

            IEnumerable<string> events;

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                events = connection.Query<string>(sql,
                    new { AggId = aggId, Version = version });
                if (events.Count() == 0)
                {
                    return null;
                }
            }

            var aggEvents = DeserializeEvents(events);
            return aggEvents;
        }

        protected internal virtual void AddAggregate(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName)
        {
            var sp = "dbo.insert_event";



            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            foreach (var pendingEvent in pendingEvents)
            {
                var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                connection.Execute(sp, new
                {
                    AggId = aggregateId,
                    AggName = aggregateName,
                    pendingEvent.EventName,
                    Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    Data = json,
                    ExpectedVersion = -1,
                    NewVersion = aggVersion
                }, commandType: CommandType.StoredProcedure);
            }
        }

        protected internal virtual void UpdateAggregate(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName)
        {
            var sp = "dbo.insert_event";


            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            var sent = 0;
            foreach (var pendingEvent in pendingEvents)
            {
                var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                connection.Execute(sp, new
                {
                    AggId = aggregateId,
                    AggName = aggregateName,
                    pendingEvent.EventName,
                    Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    Data = json,
                    ExpectedVersion = aggVersion - pendingEvents.Count + sent,
                    NewVersion = aggVersion - pendingEvents.Count + sent + 1
                }, commandType: CommandType.StoredProcedure);
                sent++;
            }
        }

        protected internal virtual void RemoveAggregate(string aggregateId)
        {
            var sp = "drop_events";


            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            connection.Execute(sp, new { AggId = aggregateId }, commandType: CommandType.StoredProcedure);
        }
    }
}