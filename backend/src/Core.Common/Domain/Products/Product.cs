using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Products
{
    public enum Condition
    {
        Used,
        New
    }

    public class Product : ValueObject
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
                if (value == null)
                {
                    throw new DomainException("Null product name");
                }

                value = value.Trim();
                _name = value.Length < NAME_MIN_LENGTH ? throw new DomainException("Too short product name") : value;
                CanonicalName = CanonicalizeProductName(_name);
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (value == null)
                {
                    throw new DomainException("Null description");
                }

                value = value.Trim();
                _description = value.Length < DESCRIPTION_MIN_LENGTH
                    ? throw new DomainException("Too short product description")
                    : value;
            }
        }

        public string CanonicalName { get; private set; }
        //TODO fix tests to allow get only
        public Condition Condition { get; internal set; }

        public Product(string name, string description, Condition condition)
        {
            Name = name;
            Description = description;
            Condition = condition;
        }

        public static string CanonicalizeProductName(string str)
        {
            return str
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToUpper())
                .OrderBy(x => x)
                .Aggregate("", (x, y) => x + " " + y)
                .Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Description;
            yield return CanonicalName;
            yield return Condition;
        }
    }
}