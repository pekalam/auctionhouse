using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Command.Mediator
{
    public interface IHTTPQueuedCommandStatusStorage
    {
        void SaveStatus(RequestStatus status, ICommand command);
        (RequestStatus, ICommand) GetCommandStatus(CorrelationId correlationId);
        void UpdateCommandStatus(RequestStatus status, ICommand command);
    }
}