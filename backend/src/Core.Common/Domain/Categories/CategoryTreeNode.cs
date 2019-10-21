using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Core.Query")]
namespace Core.Common.Domain.Categories
{
    public class CategoryTreeNode
    {
        public int CategoryId { get; }
        public string CategoryName { get; }
        public List<CategoryTreeNode> SubCategories { get; } = new List<CategoryTreeNode>();

        private int _depth = 0;
        private CategoryTreeNode _parent;

        public CategoryTreeNode(string categoryName, int categoryId, CategoryTreeNode parent)
        {
            CategoryName = categoryName;
            CategoryId = categoryId;
            if (parent != null)
            {
                _parent = parent;
                _parent.AddNode(this);
            }
        }

        private void AddNode(CategoryTreeNode node)
        {
            if (_depth + 1 == Category.MAX_CATEGORIES_DEPTH)
            {
                throw new DomainException("");
            }
            node._depth = _depth + 1;
            SubCategories.Add(node);
        }
    }
}