using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Domain.Repositories
{
    public interface IUserPaymentsRepository
    {
        Task<UserPayments> WithId(UserPaymentsId id);
        UserPayments Add(UserPayments userPayments);
    }


}
