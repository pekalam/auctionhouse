namespace Core.Common.Domain.AuctionCreateSession
{
    public interface IAuctionCreateSessionService
    {
        AuctionCreateSession GetSessionForSignedInUser();
        void SaveSessionForSignedInUser(AuctionCreateSession session);
        void RemoveSessionForSignedInUser();
        bool SessionExists();
    }
}
