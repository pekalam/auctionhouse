using Common.Application;

namespace Auctionhouse.CommandStatus
{
    public class CommandStatusDto
    {
        public string CommandId { get; set; }
        public string Status { get; set; }

        public static CommandStatusDto Create(Status status, string commandId)
        {
            return new()
            {
                CommandId = commandId,
                Status = status switch
                {
                    Common.Application.Status.COMPLETED => "COMPLETED",
                    Common.Application.Status.PENDING => "PENDING",
                    Common.Application.Status.FAILED => "FAILED",
                    _ => throw new NotImplementedException()
                }
            };
        }
    }
}
