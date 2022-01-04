namespace Web.Dto.Commands
{
    public class UpdateAuctionCommandDto
    {
        public string AuctionId { get; set; }

        //optional
        public decimal? BuyNowPrice { get; set; }
        public DateTime? EndDate { get; set; }
        //

        public List<string> Category { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string Name { get; set; }
    }

}
