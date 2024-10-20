﻿using Auctions.Application.CommandAttributes;
using Common.Application.DependencyInjection;
using Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Application.DependencyInjection
{
    public class AuctionsApplicationInstaller
    {
        public AuctionsApplicationInstaller(IServiceCollection services)
        {
            Services = services;
            AddCoreServices(services);
        }

        public IServiceCollection Services { get; }

        private static void AddCoreServices(IServiceCollection services)
        {
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers(typeof(AuctionsApplicationInstaller).Assembly);
            services.AddEventSubscribers(typeof(AuctionsApplicationInstaller));
        }

        public AuctionsApplicationInstaller AddFileStreamAccessor(Func<IServiceProvider, IFileStreamAccessor> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public AuctionsApplicationInstaller AddTempFileService(Func<IServiceProvider, ITempFileService> factory)
        {
            Services.AddTransient(factory);
            return this;
        }



        public void Build() { }
    }
}
