using Auctions.DomainEvents;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    internal static class CategoryFilterFactory
    {
        public static FilterDefinition<AuctionRead> Create(CategoryIds categoryIds) => 
            Builders<AuctionRead>.Filter.And(
                Builders<AuctionRead>.Filter.Eq(a => a.Category.Id, categoryIds.CategoryId),
                Builders<AuctionRead>.Filter.Eq(a => a.Category.SubCategory.Id, categoryIds.SubCategoryId),
                Builders<AuctionRead>.Filter.Eq(a => a.Category.SubCategory.SubCategory.Id, categoryIds.SubSubCategoryId)
            );

        public static FilterDefinition<AuctionRead> Create(int CategoryId, int SubCategoryId, int SubSubCategoryId) =>
            Builders<AuctionRead>.Filter.And(
                Builders<AuctionRead>.Filter.Eq(a => a.Category.Id, CategoryId),
                Builders<AuctionRead>.Filter.Eq(a => a.Category.SubCategory.Id, SubCategoryId),
                Builders<AuctionRead>.Filter.Eq(a => a.Category.SubCategory.SubCategory.Id, SubSubCategoryId)
            );
    }
}
