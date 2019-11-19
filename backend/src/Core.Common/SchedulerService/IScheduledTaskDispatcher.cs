using Core.Common.Command;

namespace Core.Common.SchedulerService
{
    public interface IScheduledTaskDispatcher
    {
        ICommand GetCommandFromTask(IScheduledTask scheduledTask);
    }
}
