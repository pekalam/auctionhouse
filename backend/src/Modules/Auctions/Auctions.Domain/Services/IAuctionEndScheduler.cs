
namespace Auctions.Domain.Services
{
    public interface IAuctionEndScheduler
    {
        Task<ScheduledTaskId> ScheduleAuctionEnd(Auction auction);
        Task CancelAuctionEnd(ScheduledTaskId id);
    }

    public class ScheduledTaskId
    {
        public Guid Value { get; }

        public ScheduledTaskId(Guid value)
        {
            Value = value;
        }
    }
}