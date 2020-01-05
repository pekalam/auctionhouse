using Core.Common.EventBus;

namespace Core.Common.Command
{
    public interface IHTTPQueuedCommandStatusStorage
    {
        void SaveStatus(RequestStatus status, ICommand command);
        (RequestStatus, ICommand) GetCommandStatus(CorrelationId correlationId);
        void UpdateCommandStatus(RequestStatus status, ICommand command);
    }
}