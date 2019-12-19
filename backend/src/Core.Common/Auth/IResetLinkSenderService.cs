using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Auth
{
    public interface IResetLinkSenderService
    {
        void SendResetLink(string resetId, string username, string email);
    }
}
