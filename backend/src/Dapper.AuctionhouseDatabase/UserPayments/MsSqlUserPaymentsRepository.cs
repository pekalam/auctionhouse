using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;

namespace Adapter.Dapper.AuctionhouseDatabase.UserPayments_
{
    using Core.DomainFramework;
    using global::Dapper;
    using Microsoft.Data.SqlClient;
    using Newtonsoft.Json;
    using System.Data;
    using UserPayments.Domain;

    internal class MsSqlUserPaymentsRepository : MsSqlESRepositoryBaseExceptionDecorator, IUserPaymentsRepository
    {
        private readonly AuctionhouseRepositorySettings _connectionSettings;
        private const string UserIdToUserPaymentsTable = "[dbo].[UserIdToUserPaymentsId]";

        public MsSqlUserPaymentsRepository(AuctionhouseRepositorySettings connectionSettings) : base(connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public UserPayments Add(UserPayments userPayments)
        {
            var userIdToAggregateInsert = $"INSERT INTO {UserIdToUserPaymentsTable} (UserId, AggId) VALUES (@UserId, @AggId)";

            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            AddAggregateCore(userPayments.PendingEvents, userPayments.AggregateId.Value.ToString(), userPayments.Version, "UserPayments", connection, trans);
            try
            {
                connection.Execute(userIdToAggregateInsert, new
                {
                    UserId = userPayments.UserId.Value,
                    AggId = userPayments.AggregateId.Value,
                }, transaction: trans);
            }
            catch (SqlException e) when (e.Number == 2627)
            {
                throw new ConcurrentInsertException($"Detected concurrent insert of {nameof(UserPayments)}", e);
            }
            trans.Commit();

            return userPayments;
        }

        public Task<UserPayments?> WithId(UserPaymentsId id)
        {
            var events = ReadEvents(id.Value);
            UserPayments? userPayments = events?.Count > 0 ? UserPayments.FromEvents(events) : null;
            return Task.FromResult(userPayments);
        }

        public Task<UserPayments> WithUserId(UserId id)
        {
            var sql = $"SELECT e.Data FROM dbo.Event e WHERE e.AggId = (SELECT AggId FROM {UserIdToUserPaymentsTable} WHERE UserId = @UserId) ORDER BY Version ASC";

            IEnumerable<string> events;

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                events = connection.Query<string>(sql,
                    new { UserId = id.Value });
                if (events.Count() == 0)
                {
                    return Task.FromResult((UserPayments)null!);
                }
            }

            var aggEvents = DeserializeEvents(events);
            var userPayments = UserPayments.FromEvents(aggEvents);
            return Task.FromResult(userPayments);
        }

        public UserPayments Update(UserPayments userPayments)
        {
            UpdateAggregate(userPayments.PendingEvents,
                userPayments.AggregateId.Value.ToString(),
                userPayments.Version,
                "UserPayments");
            return userPayments;
        }
    }
}
