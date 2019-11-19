using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Domain.Categories;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.Auction.Categories
{
    public class CategoriesQueryHandler : QueryHandlerBase<CategoriesQuery, CategoryTreeNode>
    {
        private readonly ICategoryTreeService _categoryTreeService;

        public CategoriesQueryHandler(ICategoryTreeService categoryTreeService)
        {
            _categoryTreeService = categoryTreeService;
        }

        protected override Task<CategoryTreeNode> HandleQuery(CategoriesQuery request, CancellationToken cancellationToken)
        {
            var categoryTreeRoot = _categoryTreeService.GetCategoriesTree();
            return Task.FromResult(categoryTreeRoot);
        }
    }
}
