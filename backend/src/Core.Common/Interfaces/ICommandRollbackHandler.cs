using Core.Common.Domain;
using Core.Common.EventBus;

namespace Core.Common.Interfaces
{
    public interface ICommandRollbackHandler
    {
        void Rollback(IAppEvent<Event> commandEvent);
    }
}
