using Common.Application;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Base.Mocks
{
    public class UnitOfWorkFactoryMock
    {
        public static Mock<IUnitOfWorkFactory> Instance
        {
            get
            {
                var unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
                unitOfWorkFactoryMock.Setup(f => f.Begin())
                    .Returns(Mock.Of<IUnitOfWork>());
                return unitOfWorkFactoryMock;
            }
        }
    }
}
