using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class CheckUsernameQueryDto
    {

        [FromQuery(Name = "username")]
        public string Username { get; set; }
    }
}
