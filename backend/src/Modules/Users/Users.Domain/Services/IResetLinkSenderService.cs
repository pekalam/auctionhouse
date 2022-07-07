using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Domain.Services
{
    public interface IResetLinkSenderService
    {
        void SendResetLink(string resetId, string username, string email);
    }
}
