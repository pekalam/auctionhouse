using System;
using Core.Command;
using Core.Command.Commands.EndAuction;
using Core.Common;
using Core.Common.Command;
using Core.Common.SchedulerService;

namespace Infrastructure.Services.SchedulerService
{
    public class ScheduledTaskDispatcher
    {
        public CommandBase GetCommandFromTask<T>(TimeTaskRequest<T> request) where T : class
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
