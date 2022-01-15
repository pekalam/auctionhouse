using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using Common.Application.Queries;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.AuthorizationRequiredAttribute_Tests
{
    [AuthorizationRequired]
    public class TestCommandBase : ICommand
    {
        public int Prop { get; set; }

        [SignedInUser]
        public Guid User { get; set; }
    }

    [AuthorizationRequired]
    public class TestQuery : IQuery<int>
    {
        public int Prop { get; set; }

        [SignedInUser]
        public Guid User { get; set; }
    }

    [AuthorizationRequired]
    public class TestCommandBaseNoUser : ICommand
    {
        public int Prop { get; set; }
    }

    [AuthorizationRequired]
    public class TestQueryNoUser : IQuery<int>
    {
        public int Prop { get; set; }
    }

    [Trait("Category", "Unit")]
    public class AuthorizationRequiredAttribute_Tests
    {
        [Fact]
        public void Loads_commands_and_queries_using_valid_combination_of_attributes_to_property_map()
        {
            AuthorizationRequiredAttribute.LoadSignedInUserCmdAndQueryMembers("Test.Common.Application");

            foreach (var cmdToProp in AuthorizationRequiredAttribute._signedInUserCommandProperties)
            {
                (typeof(ICommand).IsAssignableFrom(cmdToProp.Key) || typeof(IQuery).IsAssignableFrom(cmdToProp.Key)).Should().BeTrue();
                cmdToProp.Key.GetCustomAttributes(typeof(AuthorizationRequiredAttribute), false)
                    .Length.Should().Be(1);
            }

            AuthorizationRequiredAttribute._signedInUserCommandProperties
                .Where(pair => pair.Key.Equals(typeof(TestCommandBase)))
                .Count().Should().Be(1);

            var prop = AuthorizationRequiredAttribute._signedInUserCommandProperties
                .First(pair => pair.Key.Equals(typeof(TestCommandBase)))
                .Value;

            prop.Name.Should().Be("User");
            prop.PropertyType.Should().Be(typeof(Guid));





            AuthorizationRequiredAttribute._signedInUserQueryProperties
                .Where(pair => pair.Key.Equals(typeof(TestQuery)))
                .Count().Should().Be(1);

            prop = AuthorizationRequiredAttribute._signedInUserQueryProperties
                .First(pair => pair.Key.Equals(typeof(TestQuery)))
                .Value;

            prop.Name.Should().Be("User");
            prop.PropertyType.Should().Be(typeof(Guid));




            var nullProp = AuthorizationRequiredAttribute._signedInUserCommandProperties
                .FirstOrDefault(pair => pair.Key.Equals(typeof(TestCommandBaseNoUser)));

            nullProp.Key.Should().BeNull();
            nullProp.Value.Should().BeNull();



            nullProp = AuthorizationRequiredAttribute._signedInUserQueryProperties
                .FirstOrDefault(pair => pair.Key.Equals(typeof(TestQueryNoUser)));

            nullProp.Key.Should().BeNull();
            nullProp.Value.Should().BeNull();


        }

    }
}
