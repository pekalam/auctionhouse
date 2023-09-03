using System.Text.Json.Serialization;
using ReadModel.Contracts;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.User.UserData
{
    public class UserDataQueryResult
    {
        public string Username { get; set; }
        public UserAddress Address { get; set; }
        [JsonConverter(typeof(DecimalRoundingConverter))]
        public decimal Credits { get; set; }
    }
}
