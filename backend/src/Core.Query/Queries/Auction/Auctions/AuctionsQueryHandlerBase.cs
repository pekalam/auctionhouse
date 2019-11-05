﻿using System.Collections.Generic;
using Core.Common.Domain.Products;
using Core.Query.ReadModel;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionsQueryHandlerBase
    {
        public const int PageSize = 10;

        protected void CreateBuyNowPriceFilter(List<FilterDefinition<AuctionReadModel>> filtersArr, AuctionsQueryBase request)
        {
            if (request.MinBuyNowPrice != request.MaxBuyNowPrice
                && request.MinBuyNowPrice < request.MaxBuyNowPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                                                                     || request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly))
            {
                var filter1 = Builders<AuctionReadModel>.Filter.Gte(f => f.BuyNowPrice, request.MinBuyNowPrice);
                var filter2 = Builders<AuctionReadModel>.Filter.Lte(f => f.BuyNowPrice, request.MaxBuyNowPrice);
                var priceFilter = Builders<AuctionReadModel>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
                {
                    //buy now only
                    filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, true));
                }
                filtersArr.Add(priceFilter);
            }
        }

        protected void CreateAuctionPriceFilter(List<FilterDefinition<AuctionReadModel>> filtersArr,
            AuctionsQueryBase request)
        {
            if (request.MinAuctionPrice != request.MaxAuctionPrice
                && request.MinAuctionPrice < request.MaxAuctionPrice
                && (request.AuctionTypeQuery == AuctionTypeQuery.All || request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow
                                                                     || request.AuctionTypeQuery == AuctionTypeQuery.Auction))
            {
                var filter1 = Builders<AuctionReadModel>.Filter.Gte(f => f.ActualPrice, request.MinAuctionPrice);
                var filter2 = Builders<AuctionReadModel>.Filter.Lte(f => f.ActualPrice, request.MaxAuctionPrice);

                var priceFilter = Builders<AuctionReadModel>.Filter.And(filter1, filter2);

                if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
                {
                    filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false));
                }
                filtersArr.Add(priceFilter);
            }
        }

        protected List<FilterDefinition<AuctionReadModel>> CreateFilterDefs(AuctionsQueryBase request)
        {
            var filtersArr = new List<FilterDefinition<AuctionReadModel>>();
            if (request.AuctionTypeQuery == AuctionTypeQuery.Auction)
            {
                var f1 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionReadModel>.Filter.And(f1, f2));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.BuyNowOnly)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, true));
            }

            if (request.AuctionTypeQuery == AuctionTypeQuery.AuctionAndBuyNow)
            {
                var f1 = Builders<AuctionReadModel>.Filter.Eq(f => f.BuyNowOnly, false);
                var f2 = Builders<AuctionReadModel>.Filter.Gt(f => f.BuyNowPrice, 0);
                filtersArr.Add(Builders<AuctionReadModel>.Filter.And(f1, f2));
            }

            if (request.ConditionQuery != ConditionQuery.All)
            {
                filtersArr.Add(Builders<AuctionReadModel>.Filter.Eq(f => f.Product.Condition,
                    (Condition)request.ConditionQuery));
            }

            CreateBuyNowPriceFilter(filtersArr, request);

            CreateAuctionPriceFilter(filtersArr, request);

            return filtersArr;
        }
    }
}