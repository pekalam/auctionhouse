namespace ReadModel.Contracts.Model
{
    public class ProductRead
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Condition { get; set; }
        public string CanonicalName { get; set; }
    }
}