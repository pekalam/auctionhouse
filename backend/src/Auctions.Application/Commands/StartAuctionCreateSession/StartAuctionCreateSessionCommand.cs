using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Auctions.Application.Commands.StartAuctionCreateSession
{
    [AuthorizationRequired]
    public class StartAuctionCreateSessionCommand : ICommand
    {
    }
}