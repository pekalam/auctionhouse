using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Web.Adapters.EventSignaling
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Sid)?.Value;
        }
    }
}