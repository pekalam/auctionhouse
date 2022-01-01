using Auctions.Application;
using Auctions.Application.CommandAttributes;
using Common.Application;
using Common.Application.Commands;
using Core.Common;
using FluentAssertions;
using Moq;
using System.IO;
using System.Linq;
using Xunit;

namespace Test.Auctions.Application.SaveTempAuctionImageAttr
{
    [SaveTempAuctionImage]
    public class TestCommandBase : ICommand
    {
        [AuctionImage] public IFileStreamAccessor Img { get; set; }

        [SaveTempPath] public string Path { get; set; }
    }


    public class SaveTempAuctionImageAttribute_Tests
    {
        [Fact]
        public void LoadImagePathCommandMembers_loads_valid_commands()
        {
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers("Test.Auctions.Application");

            SaveTempAuctionImageAttribute._auctionImagePathCommandProperties.Count.Should().Be(1);
            var validPath = SaveTempAuctionImageAttribute._auctionImagePathCommandProperties.First();
            validPath.Key.Should()
                .Be(typeof(TestCommandBase));
            validPath.Value.Name.Should().Be("Path");

            SaveTempAuctionImageAttribute._auctionImageAccessorCommandProperties.Count.Should().Be(1);
            var validImg = SaveTempAuctionImageAttribute._auctionImageAccessorCommandProperties.First();
            validImg.Key.Should()
                .Be(typeof(TestCommandBase));
            validImg.Value.Name.Should().Be("Img");
        }

        [Fact]
        public void SaveImage_saves_auction_img_and_sets_img_to_null()
        {
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers("Test.Auctions.Application");

            var stubTempFileService = new Mock<ITempFileService>();
            stubTempFileService.Setup(f => f.SaveAsTempFile(It.IsAny<Stream>())).Returns("TempFile");

            var stubImplProvider = new Mock<IImplProvider>();
            stubImplProvider.Setup(f => f.Get<ITempFileService>()).Returns(stubTempFileService.Object);

            var stubStreamAccessor = new Mock<IFileStreamAccessor>();
            stubStreamAccessor.Setup(f => f.GetStream()).Returns(Stream.Null);

            var cmd = new TestCommandBase()
            {
                Img = stubStreamAccessor.Object
            };

            SaveTempAuctionImageAttribute.SaveImage(stubImplProvider.Object, cmd);

            cmd.Img.Should().BeNull();
            cmd.Path.Should().Be("TempFile");
        }
    }
}