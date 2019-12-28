using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Core.Common;
using Core.Common.Domain.Products;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Bson.Serialization.Attributes;
using Core.Common.Domain.Auctions;
using Core.Common.Query;

namespace Core.Query.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsMapperProfile : Profile
    {
        public MostViewedAuctionsMapperProfile()
        {
            CreateMap<AuctionRead, MostViewedAuctionsResult>()
                .ForMember(d => d.AuctionName, opt => opt.MapFrom(s => s.Name));
        }
    }

    public class MostViewedAuctionsQuery : IQuery<IEnumerable<MostViewedAuctionsResult>>
    {
        public const int AUCTIONS_LIMIT = 10;
        public const int VIEWS_MIN = 2;
    }

    public class MostViewedAuctionsResult
    {
        public string AuctionId { get; set; }
        public string AuctionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool BuyNowOnly { get; set; }
        [BsonDefaultValue(0)] public decimal BuyNowPrice { get; set; }
        [BsonDefaultValue(0)] public decimal ActualPrice { get; set; }
        public int TotalBids { get; set; }
        public int Views { get; set; }
        public Common.Domain.Auctions.AuctionImage[] AuctionImages { get; set; }
    }
}