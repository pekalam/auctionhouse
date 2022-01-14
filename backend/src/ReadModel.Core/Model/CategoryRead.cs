using Core.Common.Domain.Categories;

namespace ReadModel.Core.Model
{
    public class CategoryRead
    {
        public string Name { get; set; } = null!;
        public CategoryRead SubCategory { get; set; } = null!;

        public static CategoryRead FromCategory(Category category)
        {

            var catRead = new CategoryRead();
            var currentCat = category;
            var currentCatRead = catRead;
            do
            {
                currentCatRead.Name = currentCat.Name;
                if (currentCat.SubCategory is not null)
                {
                    currentCatRead.SubCategory = new CategoryRead();
                }
                currentCatRead = currentCatRead.SubCategory;
                currentCat = currentCat.SubCategory;
            } while (currentCat is not null);
            return catRead;
        }
    }
}