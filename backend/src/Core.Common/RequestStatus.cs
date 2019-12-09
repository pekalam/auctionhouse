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

        public RequestStatus(Status status, Dictionary<string, object> extraData = null)
        {
            CorrelationId = new CorrelationId(Guid.NewGuid().ToString());
            Status = status;
            ExtraData = extraData;
        }

        public RequestStatus(CorrelationId correlationId, Status status, Dictionary<string, object> extraData = null)
        {
            CorrelationId = correlationId;
            Status = status;
            ExtraData = extraData;
        }

        public static RequestStatus CreateFromCommandContext(CommandContext context, Status newStatus, Dictionary<string, object> extraData = null)
        {
            if (context == null)
            {
                return new RequestStatus(newStatus, extraData);
            }
            else
            {
                return new RequestStatus(context.CorrelationId, newStatus, extraData);
            }
        }
    }
}