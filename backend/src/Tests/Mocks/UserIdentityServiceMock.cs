using Auctions.Application;
using Common.Application;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    internal class UserIdentityServiceMock : IUserIdentityService
    {
        private readonly Guid _userId;

        public UserIdentityServiceMock(Guid userId)
        {
            _userId = userId;
        }

        public Guid GetSignedInUserIdentity()
        {
            return _userId;
        }
    }

    internal static class FileSystemFileStreamAccessor
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
