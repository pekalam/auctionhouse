using System.Xml.Schema;
using Xunit;

namespace IntegrationTests
{
    using FluentAssertions;
    using XmlCategoryTreeStore;

    public class XmlCategoryTreeStore_Tests
    {
        [Fact]
        public void GetCategoryTree_when_called_returns_valid_category_tree()
        {
            var serviceSettings = new XmlCategoryNameStoreSettings()
            {
                CategoriesFilePath = "./categoryTreeService_data/test_categories.xml",
                SchemaFilePath = "_Categories-xml-data/categories.xsd"
            };
            var testService = new XmlCategoryTreeStore(serviceSettings);
            testService.Init();

            var categories = new[] {"Fashion", "Electronics", "Sports, Hobbies & Leisure"};
            var subCategoriesCount = new[] {11, 8, 8};
            var categoryIds = new[] {0, 1, 2};
            var root = testService.GetCategoriesTree();

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

        [Theory]
        [InlineData("categoryTreeService_data/invalid_categories_dup_id.xml")]
        [InlineData("categoryTreeService_data/invalid_categories_dup_sub_id.xml")]
        [InlineData("categoryTreeService_data/invalid_categories_dup_sub_sub_id.xml")]
        public void Init_when_invalid_categories_checks_xsd_and_throws(string testFileName)
        {
            var service = new XmlCategoryTreeStore(new XmlCategoryNameStoreSettings()
            {
                CategoriesFilePath = testFileName,
                SchemaFilePath = "_Categories-xml-data/categories.xsd"
            });

            Assert.Throws<XmlSchemaValidationException>(() => service.Init());
        }
    }
}
