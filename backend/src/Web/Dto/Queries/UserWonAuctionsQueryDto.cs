using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class UserWonAuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;
    }
}