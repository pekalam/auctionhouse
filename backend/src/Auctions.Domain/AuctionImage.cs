using Core.Common.Domain;

namespace Auctions.Domain
{
    public class AuctionImageSize : ValueObject
    {
        public static readonly AuctionImageSize SIZE1 = new AuctionImageSize(720, 480);
        public static readonly AuctionImageSize SIZE2 = new AuctionImageSize(192, 108);
        public static readonly AuctionImageSize SIZE3 = new AuctionImageSize(96, 54);

        public int W { get; }
        public int H { get; }

        public AuctionImageSize(int w, int h)
        {
            W = w;
            H = h;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return W;
            yield return H;
        }
    }

    public class AuctionImage : ValueObject
    {
        public static readonly string[] AllowedExtensions = { "jpg", "png" };

        public static string GenerateImageId(AuctionImageSize size) => $"auction-img-{Guid.NewGuid().ToString()}-{size.W}";


        public string Size1Id { get; }
        public string Size2Id { get; }
        public string Size3Id { get; }

        public AuctionImage(string size1Id, string size2Id, string size3Id)
        {
            Size1Id = size1Id;
            Size2Id = size2Id;
            Size3Id = size3Id;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Size1Id;
            yield return Size2Id;
            yield return Size3Id;
        }
    }
}