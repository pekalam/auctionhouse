using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    /// <summary>
    /// Represents compound key of auction aggregate category
    /// </summary>
    public record CategoryIds(int CategoryId, int SubCategoryId, int SubSubCategoryId);

    /// <summary>
    /// Represents base domain event of auction aggregate
    /// </summary>
    public class AuctionEvent : Event
    {
        public CategoryIds CategoryIds { get; set; } = null!;

        public AuctionEvent(string eventName) : base(eventName)
        {
        }
    }
}
