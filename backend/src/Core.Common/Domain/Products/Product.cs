using Core.Common.Exceptions;

namespace Core.Common.Domain.Products
{
    public enum Condition
    {
        Used, New
    }

    public class Product
    {
        public const int NAME_MIN_LENGTH = 5;
        public const int DESCRIPTION_MIN_LENGTH = 5;

        private string _name;
        private string _description;

        public string Name
        {
            get => _name;
            set
            {
                _name = value.Length < NAME_MIN_LENGTH ? throw new DomainException("Too short product name") : value;
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value.Length < DESCRIPTION_MIN_LENGTH ? throw new DomainException("Too short product description") : value;
            }
        }
        public Condition Condition { get; set; }

        public Product(string name, string description, Condition condition)
        {
            Name = name;
            Description = description;
            Condition = condition;
        }


    }
}