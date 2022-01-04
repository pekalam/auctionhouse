namespace Auctionhouse.Command.Dto
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Condition { get; set; }
    }

    public class CreateAuctionCommandDto
    {
        public decimal? BuyNowPrice { get; set; }
        public ProductDto Product { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Category { get; set; }
        public string[] Tags { get; set; }
        public string Name { get; set; }
        public bool? BuyNowOnly { get; set; }
    }
}
