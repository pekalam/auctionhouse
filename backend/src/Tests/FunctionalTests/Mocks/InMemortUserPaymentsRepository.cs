using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests.Mocks
{
    using UserPayments.Domain.Shared;
    using UserPayments.Domain;
    using UserPayments.Domain.Repositories;

    public class InMemortUserPaymentsRepository : IUserPaymentsRepository
    {
        public static InMemortUserPaymentsRepository Instance { get; } = new InMemortUserPaymentsRepository();

        private Dictionary<UserId, UserPayments> _userIdToPayments = new();
        private Dictionary<UserPaymentsId, UserPayments> _userPaymentsIdToPayments = new();

        public IReadOnlyList<UserPayments> All => _userIdToPayments.Values.ToList();

        public UserPayments Add(UserPayments userPayments)
        {
            _userIdToPayments[userPayments.UserId] = userPayments;
            _userPaymentsIdToPayments[userPayments.AggregateId] = userPayments;
            return userPayments;
        }

        public UserPayments Update(UserPayments userPayments)
        {
            return userPayments;
        }

        public Task<UserPayments> WithId(UserPaymentsId id)
        {
            return Task.FromResult(_userPaymentsIdToPayments.ContainsKey(id) ? _userPaymentsIdToPayments[id] : null);
        }

        public Task<UserPayments> WithUserId(UserId id)
        {
            return Task.FromResult(_userIdToPayments.ContainsKey(id) ? _userIdToPayments[id] : null);
        }

        internal void Clear() => _userPaymentsIdToPayments.Clear();
    }
}
