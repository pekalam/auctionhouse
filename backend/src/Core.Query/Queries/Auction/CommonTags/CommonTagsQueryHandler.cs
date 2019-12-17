using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.CommonTags
{
    public class CommonTagsQueryHandler : QueryHandlerBase<CommonTagsQuery, Views.CommonTags>
    {
        private readonly ReadModelDbContext _dbContext;

        public CommonTagsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<Views.CommonTags> HandleQuery(CommonTagsQuery request, CancellationToken cancellationToken)
        {
            var commonTags = _dbContext.CommonTagsCollection.Find(m => m.Tag == request.Tag).FirstOrDefault();
            return Task.FromResult(commonTags);
        }
    }
}