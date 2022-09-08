using Auctions.Application;
using Auctions.Domain;
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
        }

        public static AuctionsDomainInstaller AddAuctionCreateSessionStoreAdapter(this AuctionsDomainInstaller installer)
        {
            installer.Services.AddTransient<AuctionCreateSessionStore>();
            installer.AddAuctionCreateSessionStore((prov) => prov.GetRequiredService<AuctionCreateSessionStore>());

            return installer;
        }

        public static AuctionsApplicationInstaller AddTempFileServiceAdapter(this AuctionsApplicationInstaller installer)
        {
            installer.Services.AddTransient<TempFileService>();
            installer.AddTempFileService((prov) => prov.GetRequiredService<TempFileService>());
            
            return installer;
        }

        public static AuctionsApplicationInstaller AddFileStreamAccessorAdapter(this AuctionsApplicationInstaller installer)
        {
            installer.Services.AddTransient<FileStreamAccessor>();
            installer.AddFileStreamAccessor((prov) => prov.GetRequiredService<FileStreamAccessor>());

            return installer;
        }
    }
}
