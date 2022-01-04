using Common.Application.Commands;
using Core.Command.Commands.EndAuction;

namespace QuartzTimeTaskService.AuctionEndScheduler
{
    public class ScheduledTaskDispatcher
    {
        public ICommand GetCommandFromTask<T>(TimeTaskRequest<T> request) where T : class
        {
            if (request is TimeTaskRequest<AuctionEndTimeTaskValues>)
            {
                var task = request as TimeTaskRequest<AuctionEndTimeTaskValues>;
                return new EndAuctionCommand(task.Values.AuctionId);
            }

            throw new Exception("Command not found for scheduled task");
        }
    }
}
