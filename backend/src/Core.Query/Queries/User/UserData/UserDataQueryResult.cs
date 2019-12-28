using System;
using System.Collections.Generic;
using System.Text;
using Core.Query.ReadModel;

namespace Core.Query.Queries.User.UserData
{
    public class UserDataQueryResult
    {
        public string Username { get; set; }
        public UserAddress Address { get; set; }
        public decimal Credits { get; set; }
    }
}
