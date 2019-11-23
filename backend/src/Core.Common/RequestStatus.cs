using System;
using System.Collections.Generic;
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
    }
}