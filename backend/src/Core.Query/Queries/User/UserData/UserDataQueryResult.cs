using System;
using System.Collections.Generic;
using System.Text;
using Core.Query.ReadModel;
using Newtonsoft.Json;

namespace Core.Query.Queries.User.UserData
{
    public class UserDataQueryResult
    {
        public string Username { get; set; }
        public UserAddress Address { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Credits { get; set; }
    }
}
