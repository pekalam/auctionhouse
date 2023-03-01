using Common.Application.Commands;

namespace Core.Command.Commands.EndAuction
{
    public class EndAuctionCommand : ICommand
    {
        public Guid AuctionId { get; }

        public EndAuctionCommand(Guid auctionId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandDataException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
        }
    }
}
