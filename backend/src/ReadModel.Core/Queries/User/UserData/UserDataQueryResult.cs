using System.Text.Json.Serialization;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserData
{
    public class UserDataQueryResult
    {
        public string Username { get; set; }
        public UserAddress Address { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Credits { get; set; }
    }
}
