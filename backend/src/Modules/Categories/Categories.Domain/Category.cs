namespace Core.Common.Domain.Categories
{
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
