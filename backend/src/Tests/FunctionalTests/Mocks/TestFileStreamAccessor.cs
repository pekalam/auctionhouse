using Auctions.Application;
using Moq;
using System.IO;

namespace FunctionalTests.Mocks
{
    internal static class TestFileStreamAccessor
    {
        public static IFileStreamAccessor ForFile(string filePath)
        {
            var mock = new Mock<IFileStreamAccessor>();
            mock.Setup(f => f.GetStream())
                .Returns(File.OpenRead(filePath));
            return mock.Object;
        }
    }
}
