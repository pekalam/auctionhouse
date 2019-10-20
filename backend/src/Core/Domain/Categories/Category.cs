using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Core.DomainModelTests")]
[assembly: InternalsVisibleTo("Core.Query")]
namespace Core.Common.Domain.Categories
{
    public class Category
    {
        public const int MAX_CATEGORIES_DEPTH = 6;

        public int CategoryId { get; }
        public string Name { get; }
        public Category SubCategory { get; set; }

        public Category(string name, int categoryId)
        {
            Name = name;
            CategoryId = categoryId;
        }
    }
}
