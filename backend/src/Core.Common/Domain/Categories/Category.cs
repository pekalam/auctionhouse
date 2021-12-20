using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.DomainModelTests")]
[assembly: InternalsVisibleTo("Core.Query")]
namespace Core.Common.Domain.Categories
{
    public class CategoryId : ValueObject
    {
        public int Value { get; }

        public CategoryId(int value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
        public static implicit operator CategoryId(int value) => new CategoryId(value);
        public static implicit operator int(CategoryId id) => id.Value;
    }

    public class Category : SimpleEntity<CategoryId>
    {
        public const int MAX_CATEGORIES_DEPTH = 6;

        public string Name { get; }
        public Category SubCategory { get; set; }

        public Category(string name, CategoryId id)
        {
            Name = name;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Category == false)
            {
                return false;
            }

            var cat = obj as Category;
            while (cat.SubCategory != null && this.SubCategory != null)
            {
                return this.Id == cat.Id && cat.SubCategory.Equals(this.SubCategory);
            }

            if (!(cat.SubCategory == null && this.SubCategory == null))
            {
                return false;
            }

            return cat.Id == Id;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
