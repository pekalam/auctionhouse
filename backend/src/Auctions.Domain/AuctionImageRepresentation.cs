namespace Auctions.Domain
{
    public class AuctionImageMetadata
    {
        public bool IsAssignedToAuction { get; set; } = false;
        public string Extension { get; }

        public AuctionImageMetadata(string extension)
        {
            Extension = extension;
        }
    }

    public class AuctionImageRepresentation
    {
        public AuctionImageMetadata Metadata { get; }
        public byte[] Img { get; }

        public AuctionImageRepresentation(AuctionImageMetadata metadata, byte[] img)
        {
            Metadata = metadata;
            Img = img;
        }
    }
}