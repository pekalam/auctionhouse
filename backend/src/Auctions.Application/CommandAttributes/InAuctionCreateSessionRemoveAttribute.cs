using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;


namespace Auctions.Application.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InAuctionCreateSessionRemoveAttribute : InAuctionCreateSessionAttribute
    {
        public new Action<IImplProvider, ICommand> PostHandleAttributeStrategy =>
            new Action<IImplProvider, ICommand>(RemoveAuctionCreateSession);

        private static void RemoveAuctionCreateSession(IImplProvider implProvider, ICommand commandBase)
        {
            if (_auctionCreateSessionCommandProperties.ContainsKey(commandBase.GetType()))
            {
                var auctionCreateSessionService = implProvider.Get<IAuctionCreateSessionStore>();
                auctionCreateSessionService.RemoveSession();
            }
        }
    }
}