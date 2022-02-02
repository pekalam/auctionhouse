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
        private AuctionhouseRepositorySettings _connectionSettings;

        public MsSqlESRepositoryBase(AuctionhouseRepositorySettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        protected Event DeserializeEvent(string evStr)
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

        protected List<Event> DeserializeEvents(IEnumerable<string> events)
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
            var sql = "SELECT e.Data FROM dbo.Event e WHERE e.AggId = @AggId ORDER BY Version ASC";
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
            var sql = "SELECT e.Data FROM dbo.Event e WHERE e.AggId = @AggId AND e.Version <= @Version ORDER BY Version ASC";

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

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            AddAggregateCore(pendingEvents, aggregateId, aggVersion, aggregateName, connection);
        }

        protected static void AddAggregateCore(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName, SqlConnection connection, SqlTransaction? trans = null)
        {
            var sp = "dbo.add_aggregate";

            var json = JsonConvert.SerializeObject(pendingEvents.First(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            connection.Execute(sp, new
            {
                AggId = aggregateId,
                AggregateName = aggregateName,
                pendingEvents.First().EventName,
                Data = json,
            }, transaction: trans, commandType: CommandType.StoredProcedure);
            InsertEvents(pendingEvents.Skip(1), aggregateId, aggVersion, connection);
        }

        protected internal virtual void UpdateAggregate(IReadOnlyCollection<Event> pendingEvents, string aggregateId, long aggVersion, string aggregateName)
        {
            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            InsertEvents(pendingEvents, aggregateId, aggVersion, connection);
        }

        protected static void InsertEvents(IEnumerable<Event> pendingEvents, string aggregateId, long aggVersion, SqlConnection connection)
        {
            var eventsCount = pendingEvents.Count();
            if(aggVersion - eventsCount == 0)
            {
                throw new ArgumentException($"Aggregate must be inserted first by calling {nameof(AddAggregate)}");
            }

            var sp = "dbo.insert_event";

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
                    pendingEvent.EventName,
                    Data = json,
                    ExpectedVersion = aggVersion - eventsCount + sent,
                }, commandType: CommandType.StoredProcedure);
                sent++;
            }
        }

        protected internal virtual void RemoveAggregate(string aggregateId)
        {
            var sql = "DELETE FROM Aggregate WHERE AggregateId = @AggregateId";


            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            connection.Execute(sql, new { AggregateId = aggregateId });
        }
    }
}