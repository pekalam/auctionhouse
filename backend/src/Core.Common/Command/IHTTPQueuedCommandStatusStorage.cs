
namespace Core.Common.Command
{
    public interface IHTTPQueuedCommandStatusStorage
    {
        void SaveStatus(RequestStatus status, ICommand command);
        (RequestStatus, ICommand) GetCommandStatus(CommandId commandId);
        void UpdateCommandStatus(RequestStatus status, CommandId commandId);
    }
}