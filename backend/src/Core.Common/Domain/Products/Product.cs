namespace Core.Common.Domain.Products
{
    public enum Condition
    {
        Used, New
    }

    public class Product
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Condition Condition { get; set; }

        public Product(string name, string description, Condition condition)
        {
            Name = name;
            Description = description;
            Condition = condition;
        }
    }
}