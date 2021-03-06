﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserData
{
    public class UserDataQueryHandler : QueryHandlerBase<UserDataQuery, UserDataQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IUserIdentityService _userIdentityService;

        public UserDataQueryHandler(ReadModelDbContext dbContext, IUserIdentityService userIdentityService)
        {
            _dbContext = dbContext;
            _userIdentityService = userIdentityService;
        }

        private UserIdentity GetSignedInUserIdentity()
        {
            var user = _userIdentityService.GetSignedInUserIdentity();
            if (user == null)
            {
                throw new Exception("Null user identity");
            }
            return user;
        }

        protected async override Task<UserDataQueryResult> HandleQuery(UserDataQuery request, CancellationToken cancellationToken)
        {
            var user = GetSignedInUserIdentity();

            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId, user.UserId.ToString());
            var result = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new UserDataQueryResult() { Address = model.Address, Username = model.UserIdentity.UserName, Credits = model.Credits})
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
