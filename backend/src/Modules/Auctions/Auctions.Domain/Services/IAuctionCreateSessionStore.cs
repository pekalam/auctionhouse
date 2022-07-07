namespace Auctions.Domain.Services
{
    public interface IAuctionCreateSessionStore
    {
        AuctionCreateSession GetExistingSession();
        void SaveSession(AuctionCreateSession session);
        void RemoveSession();
        bool SessionExists();
    }
}
