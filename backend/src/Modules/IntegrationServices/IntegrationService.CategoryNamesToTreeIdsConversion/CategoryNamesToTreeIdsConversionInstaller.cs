using Microsoft.Extensions.DependencyInjection;

namespace IntegrationService.CategoryNamesToTreeIdsConversion
{
    using Auctions.Domain;

    public static class CategoryNamesToTreeIdsConversionInstaller
    {
        public static AuctionsDomainInstaller AddCategoryNamesToTreeIdsConversionAdapter(this AuctionsDomainInstaller installer)
        {
            installer.Services.AddTransient<CategoryNamesToTreeIdsConversion>();

            installer.AddCategoryNamesToTreeIdsConversion((prov) => prov.GetRequiredService<CategoryNamesToTreeIdsConversion>());
            return installer;
        }
    }
}