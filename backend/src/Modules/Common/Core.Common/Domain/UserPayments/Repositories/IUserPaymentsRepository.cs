using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Domain.UserPayments.Repositories
{
    public interface IUserPaymentsRepository
    {
        Task<UserPayments> WithId(UserPaymentsId id);
        UserPayments Add(UserPayments userPayments);
    }


}
