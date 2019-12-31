using System.Collections.Generic;
using Core.Common;
using Core.Common.Domain.Products;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions
{
    public abstract class AuctionsQueryHandlerBase<T> : QueryHandlerBase<T, AuctionsQueryResult> where T : IQuery<AuctionsQueryResult>
    {
        public const int PageSize = 10;

        protected void CreateBuyNowPriceFilter(List<FilterDefinition<AuctionRead>> filtersArr, AuctionsQueryBase request)
        {
            if (request.MinBuyNowPrice != request.MaxBuyNowPrice
                && request.MinBuyNowPrice < request.MaxBuyNowPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                                                                     || request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly))
            {
                var filter1 = Builders<AuctionRead>.Filter.Gte(f => f.BuyNowPrice, request.MinBuyNowPrice);
                var filter2 = Builders<AuctionRead>.Filter.Lte(f => f.BuyNowPrice, request.MaxBuyNowPrice);
                var priceFilter = Builders<AuctionRead>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
                {
                    //buy now only
                    filtersArr.Add(Builders<AuctionRead>.Filter.Eq(f => f.BuyNowOnly, true));
                }
                filtersArr.Add(priceFilter);
            }
        }

        protected void CreateAuctionPriceFilter(List<FilterDefinition<AuctionRead>> filtersArr,
            AuctionsQueryBase request)
        {
            if (request.MinAuctionPrice != request.MaxAuctionPrice
                && request.MinAuctionPrice < request.MaxAuctionPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                                                                     || request.AuctionTypeQuery == AuctionTypeQuery.Auction))
            {
                var filter1 = Builders<AuctionRead>.Filter.Gte(f => f.ActualPrice, request.MinAuctionPrice);
                var filter2 = Builders<AuctionRead>.Filter.Lte(f => f.ActualPrice, request.MaxAuctionPrice);

                var priceFilter = Builders<AuctionRead>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
                {
                    filtersArr.Add(Builders<AuctionRead>.Filter.Eq(f => f.BuyNowOnly, false));
                }
                filtersArr.Add(priceFilter);
            }
        }

        protected List<FilterDefinition<AuctionRead>> CreateFilterDefs(AuctionsQueryBase request)
        {
            var filtersArr = new List<FilterDefinition<AuctionRead>>();

            filtersArr.Add(Builders<AuctionRead>.Filter.Eq(read => read.Archived, false));

            if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
            {
                var f1 = Builders<AuctionRead>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionRead>.Filter.Eq(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionRead>.Filter.And(f1, f2));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
            {
                filtersArr.Add(Builders<AuctionRead>.Filter.Eq(f => f.BuyNowOnly, true));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow)
            {
                var f1 = Builders<AuctionRead>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionRead>.Filter.Gt(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionRead>.Filter.And(f1, f2));
            }

            if (request.ConditionQuery != ConditionQuery.All)
            {
                filtersArr.Add(Builders<AuctionRead>.Filter.Eq(f => f.Product.Condition,
                    (Condition)request.ConditionQuery));
            }

            CreateBuyNowPriceFilter(filtersArr, request);

            CreateAuctionPriceFilter(filtersArr, request);

            return filtersArr;
        }

        protected SortDefinition<AuctionRead> GetDefaultSorting()
        {
            var sort1 = Builders<AuctionRead>.Sort.Descending(read => read.StartDate);
            var sort2 = Builders<AuctionRead>.Sort.Descending(read => read.Views);

            return Builders<AuctionRead>.Sort.Combine(sort1, sort2);
        }
    }
}