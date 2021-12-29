using System;
using System.Collections.Generic;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Common
{
    public enum Status
    {
        PENDING, FAILED, COMPLETED
    }

    public class RequestStatus
    {
        public Command.CommandId CommandId { get; }
        public Status Status { get; }
        public Dictionary<string, object> ExtraData { get; }
        //TODO
        public Exception Exception { get; }

        public RequestStatus(CommandId commandId, Status status, Dictionary<string, object> extraData = null, Exception exception = null)
        {
            CommandId = commandId;
            Status = status;
            ExtraData = extraData;
            Exception = exception;
        }

        public static RequestStatus CreateFromCommandContext(CommandContext context, Status newStatus, Dictionary<string, object> extraData = null, Exception exception = null)
        {
            return new RequestStatus(context.CommandId, newStatus, extraData, exception);
        }
    }
}