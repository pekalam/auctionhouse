using Core.Common.Domain.Categories;
using Core.DomainFramework;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.CategoriesDomain
{
    public class CategoryBuilder_Tests
    {
        private ICategoryTreeStore CreateFakeCategoryNameService()
        {
            var fakeRoot = new CategoryTreeNode(null, 0, null);
            var fakeCat0 = new CategoryTreeNode("Sport", 0, fakeRoot);
            var fakeCat1 = new CategoryTreeNode("Sport subcategory 0", 1, fakeCat0);
            var fakeCat2 = new CategoryTreeNode("Sport subcategory 1", 2, fakeCat1);

            var fakeCategoryNameService = new Mock<ICategoryTreeStore>();
            fakeCategoryNameService.Setup(f => f.GetCategoriesTree()).Returns(fakeRoot);

            return fakeCategoryNameService.Object;
        }

        [Fact]
        public void CategoryBuilder_when_valid_params_builds_category()
        {
            var stubCategoryNameService = CreateFakeCategoryNameService();

            var categoryBuilder = new CategoryBuilder(stubCategoryNameService);

            var category = categoryBuilder.FromCategoryNamesList(new List<string>()
            {
                "Sport", "Sport subcategory 0", "Sport subcategory 1"
            });

            category.SubCategory.SubCategory.SubCategory.Should().BeNull();
            category.Name.Should().Be("Sport");
            category.SubCategory.Name.Should().Be("Sport subcategory 0");
            category.SubCategory.SubCategory.Name.Should().Be("Sport subcategory 1");
        }

        [Fact]
        public void CategoryBuilder_when_invalid_subcategory_count_throws()
        {
            var stubCategoryNameService = CreateFakeCategoryNameService();

            var categoryBuilder = new CategoryBuilder(stubCategoryNameService);

            Assert.Throws<DomainException>(() =>
                categoryBuilder.FromCategoryNamesList(new List<string>()
                {
                    "Sport", "Sport subcategory 0", "Sport subcategory 1", "Invalid subcategory"
                })
            );
        }
    }
}
