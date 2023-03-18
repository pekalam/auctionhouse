using Auctions.Application.DependencyInjection;
using Auctions.DI;
using Auctions.Domain;
using Users.DI;
using Users.Domain;

namespace Auctionhouse.Command.Adapters
{
    public static class AdaptersInstaller
    {
        public static AuctionsModuleInstaller AddCommandAdapters(this AuctionsModuleInstaller auctionsInstaller)
        {
            auctionsInstaller.Domain.AddAuctionCreateSessionStoreAdapter();
            auctionsInstaller.Application.AddTempFileServiceAdapter();
            return auctionsInstaller;
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


        public static UsersModuleInstaller AddCommandAdapters(this UsersModuleInstaller usersInstaller)
        {
            AddResetLinkSenderServiceAdapter(usersInstaller.Domain);

            return usersInstaller;
        }

        private static UsersDomainInstaller AddResetLinkSenderServiceAdapter(this UsersDomainInstaller installer)
        {
            installer.Services.AddTransient<ResetLinkSenderService>();
            installer.AddResetLinkSenderService((prov) => prov.GetRequiredService<ResetLinkSenderService>());
            return installer;
        }
    }
}
