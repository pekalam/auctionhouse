using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Domain.UserPayments.Services
{
    public interface IUnitOfWork
    {
        Task Save();
    }
}
