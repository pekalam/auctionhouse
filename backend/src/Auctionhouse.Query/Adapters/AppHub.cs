using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Auctionhouse.Query
{
    [Authorize(Roles = "User")]
    public class ApplicationHub : Hub
    {
    }
}
