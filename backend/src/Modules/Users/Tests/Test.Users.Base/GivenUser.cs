using Common.Application;
using Core.Common.Domain.Users;
using Moq;
using System;
using Users.Domain;
using Users.Domain.Repositories;

namespace Users.Tests.Base
{
    public class GivenUser
    {
        private string _userName = "Marek";
        private User? _builtUser;
        private decimal _initialCredits;
        private Action? _identityServiceSetup;

        public GivenUser WithInitialCredits(decimal initialCredits)
        {
            _initialCredits = initialCredits;
            return this;
        }

        public GivenUser WithUserName(string? userName)
        {
            if(userName is null)
            {
                return this;
            }

            _userName = userName;
            return this;
        }

        public GivenUser LoggedIn(Mock<IUserIdentityService> userIdentityService)
        {
            _identityServiceSetup = () 
                => userIdentityService.Setup(f => f.GetSignedInUserIdentity()).Returns(() => _builtUser!.AggregateId);
            return this;
        }

        public User Build(IUserRepository? users = null, Mock<IUserRepository>? mockUsers = null)
        {
            if(users != null && mockUsers != null)
            {
                throw new ArgumentException();
            }

            if (_builtUser != null)
            {
                throw new InvalidOperationException();
            }

            _builtUser = User.Create(Username.Create(_userName).Result, _initialCredits);

            users?.AddUser(_builtUser);
            mockUsers?.Setup(f => f.FindUser(It.Is<UserId>(id => id == _builtUser.AggregateId))).Returns(_builtUser);

            _identityServiceSetup?.Invoke();

            return _builtUser;
        }
    }
}
