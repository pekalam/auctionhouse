using MongoDB.Driver;
using ReadModel.Contracts.Queries.Auction.CommonTags;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.CommonTags
{
    public class CommonTagsQueryHandler : QueryHandlerBase<CommonTagsQuery, Contracts.Views.CommonTags>
    {
        private readonly ReadModelDbContext _dbContext;

        public CommonTagsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<Contracts.Views.CommonTags> HandleQuery(CommonTagsQuery request, CancellationToken cancellationToken)
        {
            var commonTags = _dbContext.CommonTagsCollection.Find(m => m.Tag == request.Tag).FirstOrDefault();
            return Task.FromResult(commonTags);
        }
    }
}