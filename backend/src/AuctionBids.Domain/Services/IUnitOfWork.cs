namespace AuctionBids.Domain.Services
{
    public interface IUnitOfWork
    {
        Task Save();
    }
}
