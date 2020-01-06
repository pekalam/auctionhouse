using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Core.Common.Domain;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Repositories.SQLServer
{
    public class MsSqlESRepositoryBase
    {
        protected MsSqlConnectionSettings _connectionSettings;

        public MsSqlESRepositoryBase(MsSqlConnectionSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        private Event DeserializeEvent(string evStr)
        {
            try
            {
                Event ev = JsonConvert.DeserializeObject<Event>(evStr, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                return ev;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected List<Event> DeserializeEvents(IEnumerable<string> events)
        {
            List<Event> aggEvents = new List<Event>();
            foreach (var evStr in events)
            {
                Event ev = DeserializeEvent(evStr);
                aggEvents.Add(ev);
            }

            return aggEvents;
        }

        protected List<Event> ReadEvents(Guid aggId)
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


        protected List<Event> ReadEvents(Guid aggId, long version)
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
    }
}