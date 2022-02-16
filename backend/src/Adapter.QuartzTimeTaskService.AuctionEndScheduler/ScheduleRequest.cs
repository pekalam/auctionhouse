namespace QuartzTimeTaskService.AuctionEndScheduler
{
    public class ScheduleRequest<T> where T : class
    {
        public DateTime StartDate { get; set; }
        public string Type { get; set; }
        public string Endpoint { get; set; }
        public T Values { get; set; }
    }
}