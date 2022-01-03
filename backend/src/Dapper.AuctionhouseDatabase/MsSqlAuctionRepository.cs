using Auctions.Domain;
using Auctions.Domain.Repositories;
using Core.Common.Domain;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Dapper.AuctionhouseDatabase
{
    public class MsSqlConnectionSettings
    {
        public string ConnectionString { get; set; }
    }

    internal class MsSqlAuctionRepository : MsSqlESRepositoryBase, IAuctionRepository
    {
        public MsSqlAuctionRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
        }

        public Auction FindAuction(Guid auctionId)
        {
            List<Event>? aggEvents = ReadEvents(auctionId);
            var auction = aggEvents != null ? Auction.FromEvents(aggEvents) : null;
            return auction;
        }

        public Auction FindAuction(Guid auctionId, long version)
        {
            List<Event>? aggEvents = ReadEvents(auctionId, version);
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
                        AggId = auction.AggregateId.ToString(),
                        AggName = "Auction",
                        pendingEvent.EventName,
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
                connection.Execute(sp, new { AggId = auctionId }, commandType: CommandType.StoredProcedure);
            }
        }

        public void UpdateAuction(Auction auction)
        {
            var sp = "dbo.insert_event";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var sent = 0;
                foreach (var pendingEvent in auction.PendingEvents)
                {
                    var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    connection.Execute(sp, new
                    {
                        AggId = auction.AggregateId.ToString(),
                        AggName = "Auction",
                        pendingEvent.EventName,
                        Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                        Data = json,
                        ExpectedVersion = auction.Version - auction.PendingEvents.Count() + sent++,
                        NewVersion = auction.Version - auction.PendingEvents.Count + sent
                    }, commandType: CommandType.StoredProcedure);
                }

            }
        }
    }
}
