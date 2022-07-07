namespace Auctionhouse.Command.Dto
{
    public class ResetPasswordCommandDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetCode { get; set; }
    }
}
