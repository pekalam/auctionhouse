using Auctions.Domain;
using Auctions.Domain.Services;
using System;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    public class AuctionEndSchedulerMock : IAuctionEndScheduler
    {
        public Task CancelAuctionEnd(ScheduledTaskId id)
        {
            return Task.CompletedTask;
        }

        public Task<ScheduledTaskId> ScheduleAuctionEnd(Auction auction)
        {
            return Task.FromResult(new ScheduledTaskId(Guid.NewGuid()));
        }
    }
}
