using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class CheckUsernameQueryDto
    {
        
        [FromQuery(Name = "username")]
        public string Username { get; set; }
    }
}
