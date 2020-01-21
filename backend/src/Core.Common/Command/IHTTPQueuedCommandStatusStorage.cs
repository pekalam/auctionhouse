using Core.Common.EventBus;

namespace Core.Common.Command
{
    public interface IHTTPQueuedCommandStatusStorage
    {
        void SaveStatus(RequestStatus status, CommandBase commandBase);
        (RequestStatus, CommandBase) GetCommandStatus(CorrelationId correlationId);
        void UpdateCommandStatus(RequestStatus status, CommandBase commandBase);
    }
}