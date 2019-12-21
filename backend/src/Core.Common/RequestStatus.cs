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
        public CorrelationId CorrelationId { get; }
        public Status Status { get; }
        public Dictionary<string, object> ExtraData { get; }
        public Exception Exception { get; }

        public RequestStatus(Status status, Dictionary<string, object> extraData = null, Exception exception = null)
        {
            CorrelationId = new CorrelationId(Guid.NewGuid().ToString());
            Status = status;
            ExtraData = extraData;
            Exception = exception;
        }

        public RequestStatus(CorrelationId correlationId, Status status, Dictionary<string, object> extraData = null, Exception exception = null)
        {
            CorrelationId = correlationId;
            Status = status;
            ExtraData = extraData;
            Exception = exception;
        }

        public static RequestStatus CreateFromCommandContext(CommandContext context, Status newStatus, Dictionary<string, object> extraData = null, Exception exception = null)
        {
            if (context == null)
            {
                return new RequestStatus(newStatus, extraData, exception);
            }
            else
            {
                return new RequestStatus(context.CorrelationId, newStatus, extraData, exception);
            }
        }
    }
}