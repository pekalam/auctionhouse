using Core.Common.Domain.Categories;
using FluentAssertions;
using Xunit;

namespace Categories.Domain.Tests
{
    public class Category_Tests
    {
        [Fact]
        public void Equals_when_categories_are_equal_returns_true()
        {
            var cat1 = new Category("cat1", 1);
            cat1.SubCategory = new Category("subcat1", 2);
            cat1.SubCategory.SubCategory = new Category("subcat2_1", 3);

            var cat2 = new Category("cat1", 1);
            cat2.SubCategory = new Category("subcat1", 2);
            cat2.SubCategory.SubCategory = new Category("subcat2_1", 3);

            cat1.Equals(cat2).Should().BeTrue();
            cat2.Equals(cat1).Should().BeTrue();
        }

        [Fact]
        public void Equals_when_categories_are_not_equal_returns_false()
        {
            var cat1 = new Category("cat1", 1);
            cat1.SubCategory = new Category("subcat1", 2);
            cat1.SubCategory.SubCategory = new Category("subcat2_1", 3);

            var cat2 = new Category("cat1", 1);
            cat2.SubCategory = new Category("subcat1", 2);
            cat2.SubCategory.SubCategory = new Category("subcat2_1", 5);

            cat1.Equals(null).Should().BeFalse();
            cat1.Equals(cat2).Should().BeFalse();
            cat2.Equals(cat1).Should().BeFalse();

            cat2.SubCategory.SubCategory = null;

            cat1.Equals(cat2).Should().BeFalse();
            cat2.Equals(cat1).Should().BeFalse();


            cat2.SubCategory = null;

            cat1.Equals(cat2).Should().BeFalse();
            cat2.Equals(cat1).Should().BeFalse();

        }
    }
}