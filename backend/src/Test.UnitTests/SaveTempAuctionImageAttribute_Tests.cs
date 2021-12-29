using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Test.UnitTests.SaveTempAuctionImageAttributeTests
{
    [SaveTempAuctionImage]
    public class TestCommandBase : ICommand    {
        [AuctionImage] public IFileStreamAccessor Img { get; set; }

        [SaveTempPath] public string Path { get; set; }
    }

    [TestFixture]
    public class SaveTempAuctionImageAttribute_Tests
    {
        [Test]
        public void LoadImagePathCommandMembers_loads_valid_commands()
        {
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers("Test.UnitTests");

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

        [Test]
        public void SaveImage_saves_auction_img_and_sets_img_to_null()
        {
            SaveTempAuctionImageAttribute.LoadImagePathCommandMembers("Test.UnitTests");

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