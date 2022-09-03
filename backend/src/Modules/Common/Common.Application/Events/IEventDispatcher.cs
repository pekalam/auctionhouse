using Core.Common.Domain;

namespace Common.Application.Events
{
    public interface IEventDispatcher
    {
        Task Dispatch(IAppEvent<Event> msg);
    }

}
