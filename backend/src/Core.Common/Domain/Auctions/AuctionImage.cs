using System;

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
        public const int SIZE1_MAX_H = 1080;
        public const int SIZE1_MAX_W = 1920;
        public const int SIZE2_MAX_H = 480;
        public const int SIZE2_MAX_W = 720;
        public const int SIZE3_MAX_H = 54;
        public const int SIZE3_MAX_W = 96;


        public static string GenerateImageId(AuctionImageSize size) => $"auction-img-{Guid.NewGuid().ToString()}-{size}";


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