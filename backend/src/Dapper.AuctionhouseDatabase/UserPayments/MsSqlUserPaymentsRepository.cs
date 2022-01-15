using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;

namespace Adapter.Dapper.AuctionhouseDatabase.UserPayments_
{
    using global::Dapper;
    using Microsoft.Data.SqlClient;
    using Newtonsoft.Json;
    using System.Data;
    using UserPayments.Domain;

    internal class MsSqlUserPaymentsRepository : MsSqlESRepositoryBaseExceptionDecorator, IUserPaymentsRepository
    {
        private readonly MsSqlConnectionSettings _connectionSettings;
        private const string UserIdToUserPaymentsTable = "[dbo].[UserIdToUserPaymentsEventId]";

        public MsSqlUserPaymentsRepository(MsSqlConnectionSettings connectionSettings) : base(connectionSettings)
        {
            _connectionSettings = connectionSettings;
        }

        public UserPayments Add(UserPayments userPayments)
        {
            var sp = "dbo.insert_event";
            var userIdToAggregateInsert = $"INSERT INTO {UserIdToUserPaymentsTable} (UserId, AggId) VALUES (@UserId, @AggId)";


            using var connection = new SqlConnection(_connectionSettings.ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            foreach (var pendingEvent in userPayments.PendingEvents)
            {
                var json = JsonConvert.SerializeObject(pendingEvent, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                connection.Execute(sp, new
                {
                    AggId = userPayments.AggregateId.Value,
                    AggName = "UserPayments",
                    pendingEvent.EventName,
                    Date = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    Data = json,
                    ExpectedVersion = -1,
                    NewVersion = userPayments.Version
                }, transaction: trans, commandType: CommandType.StoredProcedure);
            }
            connection.Execute(userIdToAggregateInsert, new { 
                UserId=userPayments.UserId.Value,
                AggId=userPayments.AggregateId.Value,
            }, transaction: trans);
            trans.Commit();


            AddAggregate(userPayments.PendingEvents, userPayments.AggregateId.ToString(), userPayments.Version, "UserPayments");
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
            var sql = $"SELECT e.Data FROM dbo.Events e WHERE e.AggId = (SELECT AggId FROM {UserIdToUserPaymentsTable} WHERE UserId = @UserId)";

            IEnumerable<string> events;

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                events = connection.Query<string>(sql,
                    new { UserId = id.Value });
                if (events.Count() == 0)
                {
                    return null;
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
