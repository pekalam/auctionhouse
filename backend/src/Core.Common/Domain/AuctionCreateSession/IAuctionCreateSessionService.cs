namespace Core.Common.Domain.AuctionCreateSession
{
    public interface IAuctionCreateSessionService
    {
        AuctionCreateSession GetExistingSession();
        AuctionCreateSession StartAndSaveNewSession();
        void SaveSession(AuctionCreateSession session);
        void RemoveSession();
        bool SessionExists();
    }
}
