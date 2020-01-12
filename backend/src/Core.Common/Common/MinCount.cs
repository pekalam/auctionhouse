using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Common.Domain.Auctions;

namespace Core.Common.Common
{
    public class MinCount : ValidationAttribute
    {
        private readonly int minElements;

        public MinCount(int minElements)
        {
            this.minElements = minElements;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var collection = value as ICollection;

            if (collection?.Count < minElements)
            {
                return new ValidationResult($"{validationContext.DisplayName} does not contain at least {minElements} elements");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }

    public class MaxCount : ValidationAttribute
    {
        private readonly int _maxElements;

        public MaxCount(int maxElements)
        {
            this._maxElements = maxElements;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var collection = value as ICollection;

            if (collection?.Count > _maxElements)
            {
                return new ValidationResult($"{validationContext.DisplayName} does not contain at least {_maxElements} elements");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }

    public class ValidAuctionImageExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                if (AuctionImage.AllowedExtensions.Contains(str))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"Invalid auction image extension {str}");
                }
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
