﻿using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Auth;
using FluentAssertions;
using Infrastructure.Auth;
using Infrastructure.Repositories.SQLServer;
using NUnit.Framework;

namespace IntegrationTests
{
    public class UserAuthDataRepository_Tests
    {
        private IUserAuthenticationDataRepository _userAuthenticationDataRepository;
        private UserAuthenticationData authData = new UserAuthenticationData()
        {
            UserId = Guid.NewGuid(),
            Password = "1234",
            UserName = Guid.NewGuid().ToString().Substring(0, 15),
            Email = "mail@mail.com"
        };

        [SetUp]
        public void SetUp()
        {
            var serverOpt = new UserAuthDbContextOptions()
            {
                ConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver",
                    "Data Source=.;Initial Catalog=AuctionhouseDatabase;Integrated Security=False;User ID=sa;PWD=Qwerty1234;")
            };
            _userAuthenticationDataRepository = new UserAuthenticationDataRepository(serverOpt);
        }

        [Test]
        public void AddUserAuth_adds_auth_data_and_find_by_id_finds_it()
        {
            _userAuthenticationDataRepository.AddUserAuth(authData);

            var found = _userAuthenticationDataRepository.FindUserAuthById(authData.UserId);

            found.Should().NotBeNull();
            found.Should().BeEquivalentTo(authData);
        }

        [Test]
        public void AddUserAuth_adds_auth_data_and_find_by_username_finds_it()
        {
            _userAuthenticationDataRepository.AddUserAuth(authData);

            var found = _userAuthenticationDataRepository.FindUserAuth(authData.UserName);

            found.Should().NotBeNull();
            found.Should().BeEquivalentTo(authData);
        }

        [Test]
        public void FindUserAuth_if_auth_data_does_not_exist_returns_null()
        {
            var found = _userAuthenticationDataRepository.FindUserAuth("xxxx");

            found.Should().BeNull();
        }

        [Test]
        public void FindUserAuthById_if_auth_data_does_not_exist_returns_null()
        {
            var found = _userAuthenticationDataRepository.FindUserAuthById(Guid.NewGuid());

            found.Should().BeNull();
        }
    }
}
