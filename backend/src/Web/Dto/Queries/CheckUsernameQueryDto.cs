using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Core.Common.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class CheckUsernameQueryDto
    {
        [Required]
        [MinLength(User.MIN_USERNAME_LENGTH)]
        [FromQuery(Name = "username")]
        public string Username { get; set; }
    }
}
