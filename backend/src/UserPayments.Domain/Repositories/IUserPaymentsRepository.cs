using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Shared;

namespace UserPayments.Domain.Repositories
{
    public interface IUserPaymentsRepository
    {
        Task<UserPayments> WithId(UserPaymentsId id);
        Task<UserPayments> WithUserId(UserId id);
        UserPayments Add(UserPayments userPayments);
    }


}
