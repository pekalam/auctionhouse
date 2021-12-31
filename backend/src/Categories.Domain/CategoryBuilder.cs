using Core.DomainFramework;

namespace Core.Common.Domain.Categories
{
    public class CategoryBuilder
    {
        private readonly ICategoryTreeStore _categoryTreeService;

        public CategoryBuilder(ICategoryTreeStore categoryTreeService)
        {
            _categoryTreeService = categoryTreeService;
        }

        private List<CategoryTreeNode> GetCategoryTreeNodesFromNameList(List<string> categoryNames)
        {
            var origTree = _categoryTreeService.GetCategoriesTree();
            List<CategoryTreeNode> categories = origTree.SubCategories;
            var toReturn = new List<CategoryTreeNode>();
            int i = 0;
            string currentName = categoryNames[i];
            while (categories.Count > 0)
            {
                var matchingCategory = categories.FirstOrDefault(node => node.CategoryName == currentName);
                if (matchingCategory == null)
                {
                    throw new DomainException($"Invalid category name: {currentName}");
                }
                else
                {
                    categories = matchingCategory.SubCategories;
                    currentName = categoryNames[++i % categoryNames.Count];
                    toReturn.Add(matchingCategory);
                }
            }

            if (i < categoryNames.Count)
            {
                throw new DomainException("Invalid category names");
            }

            return toReturn;
        }

        public Category FromCategoryNamesList(List<string> categoryNames)
        {
            if (categoryNames.Count == Category.MAX_CATEGORIES_DEPTH)
            {
                throw new DomainException($"");
            }
            var treeNodes = GetCategoryTreeNodesFromNameList(categoryNames);
            var category = new Category(treeNodes[0].CategoryName, treeNodes[0].CategoryId);
            Category next = category;
            for (int i = 1; i < treeNodes.Count; i++)
            {
                next.SubCategory = new Category(treeNodes[i].CategoryName, treeNodes[i].CategoryId);
                next = next.SubCategory;
            }

            return category;
        }
    }
}