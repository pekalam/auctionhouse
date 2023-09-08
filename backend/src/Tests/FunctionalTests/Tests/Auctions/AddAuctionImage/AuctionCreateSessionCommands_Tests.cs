using FunctionalTests.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Auctions.Domain.Services;
using Auctions.DI;
using Auctions.Application.Commands.StartAuctionCreateSession;

namespace FunctionalTests.Tests.Auctions.AddAuctionImage
{
    using Adapter.AuctionImageConversion;
    using Auctionhouse.Command.Adapters;
    using FluentAssertions;
    using FunctionalTests.Mocks;
    using global::Auctions.Application.Commands.AddAuctionImage;
    using global::Auctions.Application.Commands.RemoveImage;
    using Xunit.Abstractions;

    [Trait("Category", "Functional")]
    public class AuctionCreateSessionCommands_Tests : TestBase
    {
        private const string TestImagePath = "imageData/1200x600.jpg";
        private readonly IAuctionCreateSessionStore auctionCreateSessionStore;

        public AuctionCreateSessionCommands_Tests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            auctionCreateSessionStore = ServiceProvider.GetRequiredService<IAuctionCreateSessionStore>();
        }

        protected override void ConfigureAuctionsModuleCustomDependencies(AuctionsModuleInstaller installer)
        {
            base.ConfigureAuctionsModuleCustomDependencies(installer);
            installer.Application.AddTempFileServiceAdapter();
            installer.Domain.AddAuctionImageConversionAdapter();
        }

        [Fact]
        public async Task Adds_image_to_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = TestFileStreamAccessor.ForFile(TestImagePath),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }

        [Fact]
        public async Task Removes_image_from_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = TestFileStreamAccessor.ForFile(TestImagePath),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            var removeImage = new RemoveImageCommand(0);
            await SendCommand(removeImage);

            auctionCreateSessionStore.GetExistingSession().AuctionImagesList.Any(i => i != null).Should().BeFalse();
            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }
    }
}