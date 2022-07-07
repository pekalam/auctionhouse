using Common.Application;

namespace Auctionhouse.Command.Dto
{
    public class RequestStatusDto
    {
        public string CommandId { get; set; }
        public string Status { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }

        public static explicit operator RequestStatusDto(RequestStatus response)
        {
            return new RequestStatusDto()
            {
                CommandId = response.CommandId.Id,
                Status = response.Status.ToString(),
                ExtraData = response.ExtraData
            };
        }
    }
}
