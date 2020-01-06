using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Dapper;
using Newtonsoft.Json;

namespace Infrastructure.Repositories.SQLServer
{
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

        public void UpdateUser(User user)
        {
            var sp = "dbo.insert_event";

            using (var connection = new SqlConnection(_connectionSettings.ConnectionString))
            {
                connection.Open();
                var sent = 0;
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
                        ExpectedVersion = user.Version - user.PendingEvents.Count + sent++,
                        NewVersion = user.Version - user.PendingEvents.Count + sent
                    }, commandType: CommandType.StoredProcedure);
                }

            }
        }
    }
}