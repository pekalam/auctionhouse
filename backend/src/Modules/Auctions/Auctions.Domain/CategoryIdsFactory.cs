using Auctions.DomainEvents;

namespace Auctions.Domain
{
    internal static class CategoryIdsFactory
    {
        public static CategoryIds Create(Auction auction)
        {
            return new CategoryIds(auction.Categories[0].Value, auction.Categories[1].Value, auction.Categories[2].Value);
        }
    }
}
