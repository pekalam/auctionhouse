namespace Auctions.Domain.Services
{
    public interface IAuctionBuyCancellationScheduler
    {
        void ScheduleAuctionBuyCancellation(AuctionId auctionId, TimeOnly time);
        void Cancel(AuctionId auctionId);
    }
}
