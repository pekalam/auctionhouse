namespace Core.Common.Domain.Auctions
{
    public enum AuctionImageSize
    {
        SIZE1,
        SIZE2,
        SIZE3
    }

    public class AuctionImage
    {
        public string Size1Id { get; }
        public string Size2Id { get; }
        public string Size3Id { get; }

        public AuctionImage(string size1Id, string size2Id, string size3Id)
        {
            Size1Id = size1Id;
            Size2Id = size2Id;
            Size3Id = size3Id;
        }


    }
}