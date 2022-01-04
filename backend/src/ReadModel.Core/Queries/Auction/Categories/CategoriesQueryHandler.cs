using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;

namespace ReadModel.Core.Queries.Auction.Categories
{
    public class CategoriesQueryHandler : QueryHandlerBase<CategoriesQuery, CategoryTreeNode>
    {
        private readonly ICategoryTreeStore _categoryTreeService;

        public CategoriesQueryHandler(ICategoryTreeStore categoryTreeService)
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
