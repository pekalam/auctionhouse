using System;
using Core.Command.EndAuction;
using Core.Common.Interfaces;
using Core.Common.SchedulerService;

namespace Infrastructure.Adapters.Services.SchedulerService
{
    public class ScheduledTaskDispatcher : IScheduledTaskDispatcher
    {
        public ICommand GetCommandFromTask(IScheduledTask scheduledTask)
        {
            if (scheduledTask is TimeTaskRequest<AuctionEndTimeTaskValues>)
            {
                var task = (TimeTaskRequest<AuctionEndTimeTaskValues>) scheduledTask;
                return new EndAuctionCommand(task.Values.AuctionId);
            }

            throw new Exception("Command not found for scheduled task");
        }
    }
}
