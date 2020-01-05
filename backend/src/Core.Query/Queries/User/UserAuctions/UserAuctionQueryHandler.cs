using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionsQueryHandler : QueryHandlerBase<UserAuctionsQuery, UserAuctionsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserAuctionsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserAuctionsQueryResult> HandleQuery(UserAuctionsQuery request,
            CancellationToken cancellationToken)
        {
            SortDefinition<AuctionRead> sorting = null;
            if (request.Sorting == UserAuctionsSorting.DATE_CREATED)
            {
                sorting = request.SortingDirection == UserAuctionsSortDir.ASCENDING ?
                    Builders<AuctionRead>.Sort.Ascending(read => read.DateCreated) :
                    Builders<AuctionRead>.Sort.Descending(read => read.DateCreated);
            }

            FilterDefinition<AuctionRead> filter = null;
            var idfilter =
                Builders<AuctionRead>.Filter.Eq(read => read.Creator.UserId, request.SignedInUser.UserId.ToString());

            if (request.ShowArchived)
            {
                filter = idfilter;
            }
            else
            {
                var archivedFilter = Builders<AuctionRead>.Filter.Eq(read => read.Archived, false);
                filter = Builders<AuctionRead>.Filter.And(idfilter, archivedFilter);
            }


            var count = await _dbContext.AuctionsReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                var userAuctions = await _dbContext.AuctionsReadModel
                    .Find(filter)
                    .Sort(sorting)
                    .Skip(PageSize * request.Page)
                    .Limit(PageSize)
                    .ToListAsync();
                return new UserAuctionsQueryResult() { Auctions = userAuctions, Total = count };
            }
            return new UserAuctionsQueryResult();

//
//
//
//
//            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId,
//                request.SignedInUser.UserId.ToString());
//
//            string[] idsToJoin = null;
//            var query = _dbContext.UsersReadModel
//                .Find(userReadModelFilter);
//            if (request.ShowArchived)
//            {
//                var result = await query.Project(model => new
//                    {
//                        AuctionsIds = model.CreatedAuctions
//                            .Select(s => s.AuctionId)
//                            .Skip(request.Page * PageSize)
//                            .Take(PageSize)
//                            .ToArray()
//                    })
//                    .FirstOrDefaultAsync();
//                idsToJoin = result?.AuctionsIds;
//            }
//            else
//            {
//                var result = await query.Project(model => new
//                    {
//                        AuctionsIds = model.CreatedAuctions
//                            .Where(s => s.Archived == false)
//                            .Select(s => s.AuctionId)
//                            .Skip(request.Page * PageSize)
//                            .Take(PageSize)
//                            .ToArray()
//                    })
//                    .FirstOrDefaultAsync();
//                idsToJoin = result?.AuctionsIds;
//            }
//
//            if (idsToJoin != null)
//            {
//                var countQuery = _dbContext.AuctionsReadModel
//                    .AsQueryable()
//                    .Where(read => read.UserIdentity.UserId == request.SignedInUser.UserId.ToString())
//                    .SelectMany(read => read.CreatedAuctions);
//
//                var count = 0;
//                if (request.ShowArchived)
//                {
//                    count = countQuery.Count();
//                }
//                else
//                {
//                    count = countQuery.Where(auction => auction.Archived == false)
//                        .Count();
//                }
//
//                var userAuctionsFilter = Builders<AuctionRead>.Filter
//                    .In(field => field.AuctionId, idsToJoin);
//                var userAuctions = await _dbContext.AuctionsReadModel
//                    .Find(userAuctionsFilter)
//                    .ToListAsync();
//
//                return new UserAuctionsQueryResult() {Auctions = userAuctions, Total = count};
//            }
//
//            return new UserAuctionsQueryResult();
        }
    }
}