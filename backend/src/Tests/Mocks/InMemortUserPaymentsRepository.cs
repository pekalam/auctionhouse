using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain;
using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;

namespace FunctionalTests.Mocks
{
    public class InMemortUserPaymentsRepository : IUserPaymentsRepository
    {
        private Dictionary<UserId, UserPayments.Domain.UserPayments> _userIdToPayments = new();
        private Dictionary<UserPaymentsId, UserPayments.Domain.UserPayments> _userPaymentsIdToPayments = new();

        public IReadOnlyList<UserPayments.Domain.UserPayments> All => _userIdToPayments.Values.ToList();

        public UserPayments.Domain.UserPayments Add(UserPayments.Domain.UserPayments userPayments)
        {
            _userIdToPayments[userPayments.UserId] = userPayments;
            _userPaymentsIdToPayments[userPayments.AggregateId] = userPayments;
            return userPayments;
        }

        public Task<UserPayments.Domain.UserPayments> WithId(UserPaymentsId id)
        {
            return Task.FromResult(_userPaymentsIdToPayments[id]);
        }

        public Task<UserPayments.Domain.UserPayments> WithUserId(UserId id)
        {
            return Task.FromResult(_userIdToPayments[id]);
        }
    }
}
