using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_StartAuctionCreateSession
{
    [AuthorizationRequired]
    public class StartAuctionCreateSessionCommand : ICommand
    {
        public CorrelationId CorrelationId { get; }

        public StartAuctionCreateSessionCommand(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}