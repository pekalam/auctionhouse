namespace Core.Common.Domain.Auctions
{
    public class AuctionImageMetadata
    {
        public bool IsAssignedToAuction { get; set; } = false;
    }

    public class AuctionImageRepresentation
    {
        public AuctionImageMetadata Metadata { get; set; }
        public byte[] Img { get; set; }
    }
}