using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Adapters.EventSignaling
{
    [Authorize(Roles = "User")]
    public class ApplicationHub : Hub
    {
    }
}
