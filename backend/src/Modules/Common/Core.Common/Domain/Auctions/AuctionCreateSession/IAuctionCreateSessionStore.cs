namespace Core.Common.Domain.AuctionCreateSession
{
    public interface IAuctionCreateSessionStore
    {
        AuctionCreateSession GetExistingSession();
        void SaveSession(AuctionCreateSession session);
        void RemoveSession();
        bool SessionExists();
    }
}
