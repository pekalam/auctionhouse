using Core.Common.EventBus;
using MediatR;

namespace Core.Common.Command
{
    public enum Status
    {
        PENDING, FAILED, COMPLETED
    }

    public class CommandResponse
    {
        public CorrelationId CorrelationId { get; }
        public Status Status { get; }
        public object ResponseData { get; }

        public CommandResponse(Status status, object responseData = null)
        {
            CorrelationId = new CorrelationId();
            Status = status;
            ResponseData = responseData;
        }

        public CommandResponse(CorrelationId correlationId, Status status, object responseData = null)
        {
            CorrelationId = correlationId;
            Status = status;
            ResponseData = responseData;
        }
    }

    public interface ICommand : IRequest<CommandResponse>
    {
    }
}
