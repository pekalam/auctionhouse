using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    using AuctionBids.Domain;
    using Core.Common.Domain;
    using global::Dapper;
    using Microsoft.Data.SqlClient;

    internal class MsSqlAuctionBidsRepository : MsSqlESRepositoryBaseExceptionDecorator, IAuctionBidsRepository
    {
        private const string AggregateName = "AuctionBids";
        private const string AuctionIdToAuctionBidsIdTable = "[dbo].[AuctionIdToAuctionBidsId]";

        private readonly AuctionhouseRepositorySettings _connectionSettings; //TODO

        public MsSqlAuctionBidsRepository(AuctionhouseRepositorySettings connectionSettings) : base(connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public void Add(AuctionBids auctionBids)
        {
            var auctionIdToAuctionBidsIdInsert = $"INSERT INTO {AuctionIdToAuctionBidsIdTable} (AuctionId, AggregateId) VALUES (@AuctionId, @AggregateId)";
            
            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            AddAggregateCore(auctionBids.PendingEvents, auctionBids.AggregateId.ToString()!, auctionBids.Version, AggregateName, connection, trans);
            connection.Execute(auctionIdToAuctionBidsIdInsert, new
            {
                AuctionId = auctionBids.AuctionId.ToString(),
                AggregateId = auctionBids.AggregateId.ToString(),
            }, transaction: trans);
            trans.Commit();
        }

        public void Update(AuctionBids auctionBids)
        {
            UpdateAggregate(auctionBids.PendingEvents, auctionBids.AggregateId.ToString(), auctionBids.Version, AggregateName);
        }

        public AuctionBids? WithAuctionId(AuctionId auctionId)
        {
            var sql = $"SELECT e.Data FROM dbo.Event e WHERE e.AggId = (SELECT AggregateId FROM {AuctionIdToAuctionBidsIdTable} WHERE AuctionId = @AuctionId) ORDER BY Version ASC";

            IEnumerable<string> events;

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                events = connection.Query<string>(sql,
                    new { AuctionId = auctionId.Value.ToString() });
                if (events.Count() == 0)
                {
                    return null;
                }
            }

            var aggEvents = DeserializeEvents(events);
            var auctionBids = AuctionBids.FromEvents(aggEvents);
            return auctionBids;
        }
    }
}
