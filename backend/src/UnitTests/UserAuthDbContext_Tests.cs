using System;
using FluentAssertions;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using Castle.DynamicProxy.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Infrastructure.UnitTests
{
    [TestFixture]
    public class UserAuthDbContext_Tests
    {
        [Test]
        public void Seed_when_true_seeds_with_test_data()
        {
            ServiceProviderCache.Instance.GetType()
                .GetAllFields()[1].SetValue(ServiceProviderCache.Instance, new ServiceProviderCache());

            UsertAuthDbContext.Seed = true;
            var opt = new DbContextOptionsBuilder()
                .UseInMemoryDatabase("test")
                .Options;

            using (var context = new UsertAuthDbContext(opt))
            {
                context.Database.EnsureCreated();
                var testUser = context.UserAuth.FirstOrDefault();

                testUser.UserName.Should().Be("test");
                testUser.Password.Should().Be("pass");
                testUser.UserId.Should().NotBeEmpty();
                context.Database.EnsureDeleted();
            }
        }
    }
}
