using Core.Common.Interfaces;

namespace Core.Common.SchedulerService
{
    public interface IScheduledTaskDispatcher
    {
        ICommand GetCommandFromTask(IScheduledTask scheduledTask);
    }
}
