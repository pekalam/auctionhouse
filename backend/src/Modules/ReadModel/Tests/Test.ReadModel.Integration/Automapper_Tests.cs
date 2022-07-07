using AutoMapper;
using ReadModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.ReadModel.Integration
{
    public class Automapper_Tests
    {
        [Fact]
        public void f()
        {
            MapperConfigHolder.Configuration.AssertConfigurationIsValid();
        }
    }
}
