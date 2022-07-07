using Core.Common.Domain;
using Core.Common.EventBus;

namespace Core.Common.Command
{
    public interface ICommandRollbackHandler
    {
        void Rollback(IAppEvent<Event> commandEvent);
    }
}
