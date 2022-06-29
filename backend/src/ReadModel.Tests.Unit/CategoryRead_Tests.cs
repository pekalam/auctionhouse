using Core.Common.Domain.Categories;
using FluentAssertions;
using ReadModel.Core.Model;
using Xunit;

namespace ReadModel.Tests.Unit
{
    public class CategoryRead_Tests
    {
        [Fact]
        public void FromCategory_ValidCategory_ReturnsValidCategoryRead()
        {
            var cat = new Category("test1", new CategoryId(1));
            cat.SubCategory = new Category("test2", new CategoryId(2));
            cat.SubCategory.SubCategory = new Category("test3", new CategoryId(3));

            var catRead = CategoryRead.FromCategory(cat);
            
            catRead.Id.Should().Be(1);
            catRead.Name.Should().Be("test1");
            catRead.SubCategory.Id.Should().Be(2);
            catRead.SubCategory.Name.Should().Be("test2");
            catRead.SubCategory.SubCategory.Id.Should().Be(3);
            catRead.SubCategory.SubCategory.Name.Should().Be("test3");
        }
    }
}