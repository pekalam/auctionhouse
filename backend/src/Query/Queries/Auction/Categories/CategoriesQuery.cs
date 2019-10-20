using Core.Common.Domain.Categories;
using MediatR;

namespace Core.Query.Queries.Auction.Categories
{
    public class CategoriesQuery : IRequest<CategoryTreeNode>
    {
    }
}
