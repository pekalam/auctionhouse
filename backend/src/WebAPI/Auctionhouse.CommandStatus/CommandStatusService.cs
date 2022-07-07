using Common.Application;
using Microsoft.Data.SqlClient;

namespace Auctionhouse.CommandStatus
{
    public class CommandStatusServiceOptions
    {
        public string ConnectionString { get; set; } = null!;
    }

    public class CommandStatusService
    {
        private readonly CommandStatusServiceOptions _options;

        public CommandStatusService(CommandStatusServiceOptions options)
        {
            _options = options;
        }

        public async Task<CommandStatusDto?> CheckStatus(string commandId)
        {
            using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Completed, Failed, (SELECT COUNT(Id) from dbo.SagaEventsToConfirm where Processed = 0 and CorrelationId = sec.CorrelationId) as UnprocessedEvents FROM dbo.SagaEventsConfirmations sec WHERE CommandId = @CommandId";
            cmd.Parameters.AddWithValue("@CommandId", commandId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return null;
            }
            var completed = reader.GetBoolean(0);
            var failed = reader.GetBoolean(1);
            var unprocessedEvents = reader.GetInt32(2);

            if(unprocessedEvents == 0)
            {
                if(completed && !failed)
                {
                    return CommandStatusDto.Create(Status.COMPLETED, commandId);
                }
                if(!completed && failed)
                {
                    return CommandStatusDto.Create(Status.FAILED, commandId);
                }
            }
            return CommandStatusDto.Create(Status.PENDING, commandId);
        }
    }
}
