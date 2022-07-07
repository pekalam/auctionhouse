using Auctions.Domain;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.CommandAttributes
{
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
