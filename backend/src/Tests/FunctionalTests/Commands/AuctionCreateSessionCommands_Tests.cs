using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.Commands
{
    using Auctions.Application.Commands.AddAuctionImage;
    using Auctions.Application.Commands.RemoveImage;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.Domain;
    using Auctions.Domain.Services;
    using Core.Common;
    using FluentAssertions;
    using FunctionalTests.Mocks;
    using Moq;
    using System.IO;
    using Xunit.Abstractions;

    [Trait("Category","Functional")]
    public class AuctionCreateSessionCommands_Tests : TestBase
    {
        private InMemAuctionCreateSessionStore auctionCreateSessionStore;
        private Mock<ITempFileService> tempFileServiceMock;
        private Mock<IAuctionImageConversion> imageConversionMock;

        public AuctionCreateSessionCommands_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application")
        {
            auctionCreateSessionStore = (InMemAuctionCreateSessionStore)ServiceProvider.GetRequiredService<IAuctionCreateSessionStore>();
            tempFileServiceMock = new Mock<ITempFileService>();
            tempFileServiceMock.Setup(f => f.SaveAsTempFile(It.IsAny<Stream>()))
                .Callback<Stream>(stream => {
                    using var newFile = File.Create("tmp.jpg"); 
                    stream.CopyTo(newFile);
                })
                .Returns("tmp.jpg");
            imageConversionMock = new Mock<IAuctionImageConversion>();
            imageConversionMock.Setup(f => f.ValidateImage(It.IsAny<AuctionImageRepresentation>(), It.IsAny<string[]>()))
                .Returns(true);
            imageConversionMock.Setup(f => f.ConvertTo(It.IsAny<AuctionImageRepresentation>(), It.IsAny<AuctionImageSize>()))
                .Returns<AuctionImageRepresentation, AuctionImageSize>((i, sz) => i);
        }

        protected override void AddServices(IServiceCollection services)
        {
            services.AddTransient((s) => tempFileServiceMock.Object);
            services.AddTransient((s) => imageConversionMock.Object);
        }

        [Fact]
        public async Task Adds_image_to_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = FileSystemFileStreamAccessor.ForFile("imageData/1200x600.jpg"),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            tempFileServiceMock.Verify(f => f.SaveAsTempFile(It.IsAny<Stream>()), Times.Once());
            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }

        [Fact]
        public async Task Removes_image_from_AuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);

            var addImg = new AddAuctionImageCommand()
            {
                Img = FileSystemFileStreamAccessor.ForFile("imageData/1200x600.jpg"),
                Extension = "jpg",
                ImgNum = 0,
            };
            await SendCommand(addImg);

            var removeImage = new RemoveImageCommand(0);
            await SendCommand(removeImage);

            auctionCreateSessionStore.GetExistingSession().AuctionImages.Any(i => i != null).Should().BeFalse();
            auctionCreateSessionStore.GetExistingSession().Should().NotBeNull();
        }
    }
}