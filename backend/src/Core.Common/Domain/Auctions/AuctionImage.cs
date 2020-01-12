using System;

namespace Core.Common.Domain.Auctions
{
    public class AuctionImageSize
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
    }

    public class AuctionImage
    {
        public static readonly string[] AllowedExtensions = {"jpg", "png"};

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


    }
}