using System.Threading.Tasks;
using Core.Common.Domain.Auctions;

namespace Core.Common.SchedulerService
{
    public interface IAuctionSchedulerService
    {
        Task<ScheduledTaskId> ScheduleAuctionEndTask(Auction auction);
        Task CancelAuctionEndTask(ScheduledTaskId id);
    }
}