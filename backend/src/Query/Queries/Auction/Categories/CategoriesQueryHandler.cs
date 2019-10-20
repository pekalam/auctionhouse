using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Categories;
using MediatR;

namespace Core.Query.Queries.Auction.Categories
{
    public class CategoriesQueryHandler : IRequestHandler<CategoriesQuery, CategoryTreeNode>
    {
        private readonly ICategoryTreeService _categoryTreeService;

        public CategoriesQueryHandler(ICategoryTreeService categoryTreeService)
        {
            _categoryTreeService = categoryTreeService;
        }

        public Task<CategoryTreeNode> Handle(CategoriesQuery request, CancellationToken cancellationToken)
        {
            var categoryTreeRoot = _categoryTreeService.GetCategoriesTree();
            return Task.FromResult(categoryTreeRoot);
        }
    }
}
