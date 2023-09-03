using AutoMapper;
using Core.Common.Domain.Categories;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.Auctions;
using ReadModel.Contracts.Queries.Auction.Auctions.ByCategory;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQueryHandler : AuctionsQueryHandlerBase<AuctionsByCategoryQuery>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IMapper _mapper;

        public AuctionsByCategoryQueryHandler(ReadModelDbContext dbContext, CategoryBuilder categoryBuilder, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected async override Task<AuctionsQueryResult> HandleQuery(AuctionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var filtersArr = new List<FilterDefinition<AuctionRead>>()
            {
                Builders<AuctionRead>.Filter.AuctionIsNotLocked(),
                Builders<AuctionRead>.Filter.Eq(f => f.Category.Name, request.CategoryNames[0]),
                Builders<AuctionRead>.Filter.Eq(f => f.Category.SubCategory.Name, request.CategoryNames[1]),
                Builders<AuctionRead>.Filter.Eq(f => f.Category.SubCategory.SubCategory.Name, request.CategoryNames[2]),
            };
            var filtersFromBase = CreateFilterDefs(request);
            filtersArr.AddRange(filtersFromBase);

            var auctions = await _dbContext.AuctionsReadModel
                .Find(Builders<AuctionRead>.Filter.And(filtersArr))
                .Skip(request.Page * PageSize)
                .Project(model => _mapper.Map<AuctionListItem>(model))
                .Limit(PageSize)
                .Sort(GetDefaultSorting())
                .ToListAsync();

            long total = await _dbContext.AuctionsReadModel.CountDocumentsAsync(Builders<AuctionRead>.Filter.And(filtersArr));

            return new AuctionsQueryResult()
            {
                Auctions = auctions,
                Total = total
            };
        }
    }
}