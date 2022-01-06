namespace ReadModel.Core.Model
{
    public class CategoryRead
    {
        public string Name { get; set; }
        public CategoryRead SubCategory { get; set; }
    }
}