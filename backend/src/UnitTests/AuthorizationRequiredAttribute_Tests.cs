using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Command;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.Query;
using FluentAssertions;
using FluentAssertions.Common;
using NUnit.Framework;

namespace UnitTests.AuthorizationRequiredAttribute_Tests
{
    [AuthorizationRequired]
    public class TestCommand : ICommand
    {
        public int Prop { get; set; }

        [SignedInUser]
        public UserIdentity User { get; set; }
    }

    [AuthorizationRequired]
    public class TestQuery : IQuery<int>
    {
        public int Prop { get; set; }

        [SignedInUser]
        public UserIdentity User { get; set; }
    }

    [AuthorizationRequired]
    public class TestCommandNoUser : ICommand
    {
        public int Prop { get; set; }
    }

    [AuthorizationRequired]
    public class TestQueryNoUser : IQuery<int>
    {
        public int Prop { get; set; }
    }

    [TestFixture]
    public class AuthorizationRequiredAttribute_Tests
    {
        [Test]
        public void LoadSignedInUserCmdMembers_Loads_commands_and_queries_to_signed_in_property_map()
        {
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers("UnitTests");

            foreach (var cmdToProp in AuthorizationRequiredAttribute._signedInUserCommandProperties)
            {
                (cmdToProp.Key.Implements(typeof(ICommand)) || cmdToProp.Key.Implements(typeof(IQuery))).Should().BeTrue();
                cmdToProp.Key.GetCustomAttributes(typeof(AuthorizationRequiredAttribute), false)
                    .Length.Should().Be(1);
            }

            AuthorizationRequiredAttribute._signedInUserCommandProperties
                .Where(pair => pair.Key.Equals(typeof(TestCommand)))
                .Count().Should().Be(1);

            var prop = AuthorizationRequiredAttribute._signedInUserCommandProperties
                .First(pair => pair.Key.Equals(typeof(TestCommand)))
                .Value;

            prop.Name.Should().Be("User");
            prop.PropertyType.Should().Be(typeof(UserIdentity));





            AuthorizationRequiredAttribute._signedInUserQueryProperties
                .Where(pair => pair.Key.Equals(typeof(TestQuery)))
                .Count().Should().Be(1);

            prop = AuthorizationRequiredAttribute._signedInUserQueryProperties
                .First(pair => pair.Key.Equals(typeof(TestQuery)))
                .Value;

            prop.Name.Should().Be("User");
            prop.PropertyType.Should().Be(typeof(UserIdentity));




            var nullProp = AuthorizationRequiredAttribute._signedInUserCommandProperties
                .FirstOrDefault(pair => pair.Key.Equals(typeof(TestCommandNoUser)));

            nullProp.Key.Should().BeNull();
            nullProp.Value.Should().BeNull();



            nullProp = AuthorizationRequiredAttribute._signedInUserQueryProperties
                .FirstOrDefault(pair => pair.Key.Equals(typeof(TestQueryNoUser)));

            nullProp.Key.Should().BeNull();
            nullProp.Value.Should().BeNull();


        }

    }
}
