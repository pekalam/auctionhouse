using Auctions.Domain.Services;
using Common.Application;
using Common.WebAPI.Auth;
using Core.Common;
using Users.Domain.Services;

namespace Auctionhouse.Command.Adapters
{
    internal static class AdaptersInstaller
    {
        public static void AddWebApiAdapters(this IServiceCollection services)
        {
            services.AddTransient<JwtService>();
            services.AddTransient<ITempFileService, TempFileService>();
            services.AddTransient<IResetLinkSenderService, ResetLinkSenderService>();
            services.AddTransient<IAuctionCreateSessionStore, AuctionCreateSessionStore>();
            services.AddTransient<IConvertCategoryNamesToRootToLeafIds, ConvertCategoryNamesToRootToLeafIds>();
        }
    }
}
