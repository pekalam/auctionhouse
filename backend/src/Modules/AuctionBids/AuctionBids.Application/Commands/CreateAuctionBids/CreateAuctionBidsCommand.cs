using Common.Application.Commands;

namespace AuctionBids.Application.Commands.CreateAuctionBids
{
    public class CreateAuctionBidsCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubSubCategoryId { get; set; }
        public Guid Owner { get; set; }
    }
}
