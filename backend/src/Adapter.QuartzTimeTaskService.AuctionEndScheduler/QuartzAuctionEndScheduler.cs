using Auctions.Domain;
using Auctions.Domain.Services;

namespace QuartzTimeTaskService.AuctionEndScheduler
{
    internal class QuartzAuctionEndScheduler : IAuctionEndScheduler
    {
        private readonly ITimeTaskClient _timeTaskClient;
        private readonly TimeTaskServiceSettings _serviceSettings;

        public QuartzAuctionEndScheduler(ITimeTaskClient timeTaskClient, TimeTaskServiceSettings serviceSettings)
        {
            _timeTaskClient = timeTaskClient;
            _timeTaskClient.ApiKey = serviceSettings.ApiKey;
            _serviceSettings = serviceSettings;
        }

        protected virtual ScheduleRequest<AuctionEndTimeTaskValues> CreateScheduleRequest(Auction auction)
        {
            var sheduleRequest = new ScheduleRequest<AuctionEndTimeTaskValues>();
            sheduleRequest.StartDate = auction.EndDate.Value;
            sheduleRequest.Endpoint = _serviceSettings.AuctionEndEchoTaskEndpoint;
            sheduleRequest.Type = "echo";
            sheduleRequest.Values = new AuctionEndTimeTaskValues()
            {
                AuctionId = auction.AggregateId.Value
            };
            return sheduleRequest;
        }

        public Task<ScheduledTaskId> ScheduleAuctionEnd(Auction auction)
        {
            var sheduleRequest = CreateScheduleRequest(auction);
            var infoResponse = _timeTaskClient.ScheduleTask(sheduleRequest).Result;

            return Task.FromResult(new ScheduledTaskId(infoResponse.Id));
        }

        public Task CancelAuctionEnd(ScheduledTaskId id)
        {
            return _timeTaskClient.CancelTask(new CancelTaskRequest()
            {
                Id = id.Value,
                Type = "echo"
            });
        }
    }
}
