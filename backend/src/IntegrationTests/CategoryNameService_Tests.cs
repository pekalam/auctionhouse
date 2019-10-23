using FluentAssertions;
using Infrastructure.Services;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class CategoryNameService_Tests
    {
        private CategoryNameServiceSettings _serviceSettings;
        private CategoryTreeService _testService;

        [SetUp]
        public void SetUp()
        {
            _serviceSettings = new CategoryNameServiceSettings()
            {
                CategoriesFilePath = "./test_categories.xml"
            };
            _testService = new CategoryTreeService(_serviceSettings);
            _testService.Init();
        }

        [Test]
        public void GetCategoryTree_when_called_returns_valid_category_tree()
        {
            var categories = new[] {"Fashion", "Electronics", "Sports, Hobbies & Leisure"};
            var subCategoriesCount = new[] {11, 8, 8};
            var categoryIds = new[] {0, 1, 2};
            var root = _testService.GetCategoriesTree();

            for(int i = 0; i < root.SubCategories.Count; i++)
            {
                var category = root.SubCategories[i];

                category.CategoryId.Should().Be(categoryIds[i]);
                category.CategoryName.Should().Be(categories[i]);
                category.SubCategories.Count.Should().Be(subCategoriesCount[i]);
                for (int j = 0; j < category.SubCategories.Count; j++)
                {
                    var subCategory = category.SubCategories[j];
                    subCategory.CategoryName.Should().NotBeNullOrEmpty();
                    subCategory.SubCategories.Count.Should().Be(2);
                    for (int k = 0; k < subCategory.SubCategories.Count; k++)
                    {
                        var subSubCat = subCategory.SubCategories[k];
                        subSubCat.CategoryName.Should().NotBeNullOrEmpty();
                    }
                }
            }

            root.SubCategories.Count.Should().Be(3);
            root.CategoryName.Should().BeNull();
        }
    }
}
