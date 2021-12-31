using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Domain.Services
{
    public interface IUnitOfWork
    {
        Task Save();
    }
}
