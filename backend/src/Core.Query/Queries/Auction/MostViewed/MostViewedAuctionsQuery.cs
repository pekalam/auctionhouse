using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common.Domain.Products;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Core.Common.Domain.Auctions;

namespace Core.Query.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsMapperProfile : Profile
    {
        public MostViewedAuctionsMapperProfile()
        {
            CreateMap<AuctionRead, MostViewedAuctionsResult>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
        }
    }

    public class MostViewedAuctionsQuery : IRequest<IEnumerable<MostViewedAuctionsResult>>
    {
        public const int AUCTIONS_LIMIT = 10;
        public const int VIEWS_MIN = 2;
    }

    public class MostViewedAuctionsQueryHandler : IRequestHandler<MostViewedAuctionsQuery, IEnumerable<MostViewedAuctionsResult>>
    {
        private readonly ReadModelDbContext _dbContext;

        public MostViewedAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MostViewedAuctionsResult>> Handle(MostViewedAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            var mapper = MapperConfigHolder.Configuration.CreateMapper();

            var result = await _dbContext.AuctionsReadModel
                .Find(model => model.Views > MostViewedAuctionsQuery.VIEWS_MIN)
                .Project(model => mapper.Map<MostViewedAuctionsResult>(model))
                .Limit(MostViewedAuctionsQuery.AUCTIONS_LIMIT)
                .ToListAsync();

            return result;
        }
    }

    public class MostViewedAuctionsResult
    {
        public string AuctionId { get; set; }
        public string ProductName { get; set; }
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