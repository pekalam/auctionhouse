using Common.Application.Commands;

namespace Common.Application
{
    public enum Status
    {
        PENDING, FAILED, COMPLETED
    }

    public class RequestStatus
    {
        public CommandId CommandId { get; }
        public Status Status { get; private set; }
        public Dictionary<string, object>? ExtraData { get; private set; }
        public Exception? Exception { get; set; }

        public RequestStatus(CommandId commandId, Status status, Dictionary<string, object>? extraData = null, Exception? exception = null)
        {
            CommandId = commandId;
            Status = status;
            ExtraData = extraData;
            Exception = exception;
        }

        public void MarkAsCompleted()
        {
            if (Status != Status.PENDING)
            {
                return;
            }
            Status = Status.COMPLETED;
        }

        public void MarkAsFailed()
        {
            if (Status != Status.PENDING)
            {
                return;
            }
            Status = Status.FAILED;
        }

        public void SetExtraData(Dictionary<string, object>? extraData)
        {
            ExtraData = extraData;
        }

        public static RequestStatus CreateFromCommandContext(CommandContext commandContext, Status status) =>
            new RequestStatus(commandContext.CommandId, status);

        public static RequestStatus CreatePending(CommandContext context)
        {
            return new RequestStatus(context.CommandId, Status.PENDING);
        }
    }
}