using Microsoft.Extensions.DependencyInjection;
using ReadModel.Contracts.Services;
using System;

namespace ReadModel.Contracts
{
    public class ReadModelInstaller
    {
        public IServiceCollection Services { get; }

        public ReadModelInstaller(IServiceCollection services)
        {
            Services = services;
        }

        public ReadModelInstaller AddBidRaisedNotifications(Func<IServiceProvider, IBidRaisedNotifications> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public ReadModelInstaller AddAuctionImageReadRepository(Func<IServiceProvider, IAuctionImageReadRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
