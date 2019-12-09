using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Repositories.SQLServer
{
    public class MsSqlConnectionSettings
    {
        public string ConnectionString { get; set; }
    }

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

        protected List<Event> ReadEvents(Guid aggId)
        {
            var sql = "SELECT e.Data FROM dbo.Events e WHERE e.AggId = @AggId";
            IEnumerable<string> events;
            List<Event> aggEvents = new List<Event>();
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

            foreach (var evStr in events)
            {
                Event ev = DeserializeEvent(evStr);
                aggEvents.Add(ev);
            }

            return aggEvents;
        }


        protected List<Event> ReadEvents(Guid aggId, long version)
        {
            var sql = "SELECT e.Data FROM dbo.Events e WHERE e.AggId = @AggId AND e.Version <= @Version";
            IEnumerable<string> events;
            List<Event> aggEvents = new List<Event>();
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

            foreach (var evStr in events)
            {
                Event ev = DeserializeEvent(evStr);
                aggEvents.Add(ev);
            }

            return aggEvents;
        }
    }

    public class MsSqlAuctionRepository : MsSqlESRepositoryBase, IAuctionRepository
    {
        public MsSqlAuctionRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public Auction FindAuction(Guid auctionId)
        {
            List<Event> aggEvents = ReadEvents(auctionId);
            var auction = aggEvents != null ? Auction.FromEvents(aggEvents) : null;
            return auction;
        }

        public Auction FindAuction(Guid auctionId, long version)
        {
            List<Event> aggEvents = ReadEvents(auctionId, version);
            var auction = aggEvents != null ? Auction.FromEvents(aggEvents) : null;
            return auction;
        }

        public Auction AddAuction(Auction auction)
        {
            var sp = "dbo.insert_event";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                foreach (var pendingEvent in auction.PendingEvents)
                {
                    var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    connection.Execute(sp, new
                    {
                        AggId = auction.AggregateId,
                        AggName = "Auction",
                        EventName = pendingEvent.EventName,
                        Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                        Data = json,
                        ExpectedVersion = -1,
                        NewVersion = auction.Version
                    }, commandType: CommandType.StoredProcedure);
                }

            }

            return auction;
        }

        public void RemoveAuction(Guid auctionId)
        {
            var sp = "drop_events";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                connection.Execute(sp, new {AggId = auctionId}, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateAuction(Auction auction)
        {
            var sp = "dbo.insert_event";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                foreach (var pendingEvent in auction.PendingEvents)
                {
                    var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    connection.Execute(sp, new
                    {
                        AggId = auction.AggregateId,
                        AggName = "Auction",
                        EventName = pendingEvent.EventName,
                        Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                        Data = json,
                        ExpectedVersion = auction.Version - auction.PendingEvents.Count(),
                        NewVersion = auction.Version
                    }, commandType: CommandType.StoredProcedure);
                }

            }
        }
    }

    public class MsSqlUserRepository : MsSqlESRepositoryBase, IUserRepository
    {
        public MsSqlUserRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public User AddUser(User user)
        {
            var sp = "dbo.insert_event";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                foreach (var pendingEvent in user.PendingEvents)
                {
                    var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    connection.Execute(sp, new
                    {
                        AggId = user.AggregateId,
                        AggName = "User",
                        EventName = pendingEvent.EventName,
                        Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                        Data = json,
                        ExpectedVersion = -1,
                        NewVersion = user.Version
                    }, commandType: CommandType.StoredProcedure);
                }

            }

            return user;
        }

        public User FindUser(UserIdentity userIdentity)
        {
            List<Event> aggEvents = ReadEvents(userIdentity.UserId);
            User user = aggEvents != null ? User.FromEvents(aggEvents) : null;
            return user;
        }
    }
}
