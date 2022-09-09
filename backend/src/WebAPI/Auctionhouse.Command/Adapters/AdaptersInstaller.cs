using Auctions.Application;
using Auctions.Domain;
using Users.Domain;

namespace Auctionhouse.Command.Adapters
{
    internal static class AdaptersInstaller
    {
        public static UsersDomainInstaller AddResetLinkSenderServiceAdapter(this UsersDomainInstaller installer)
        {
            installer.Services.AddTransient<ResetLinkSenderService>();
            installer.AddResetLinkSenderService((prov) => prov.GetRequiredService<ResetLinkSenderService>());
            return installer;
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
