using System;
using Core.Command;
using Core.Command.Commands.EndAuction;
using Core.Common;
using Core.Common.Command;
using Core.Common.SchedulerService;

namespace Infrastructure.Services.SchedulerService
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
