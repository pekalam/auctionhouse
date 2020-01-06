using System.Threading.Tasks;
using Core.Common.Domain.Auctions;
using Core.Common.SchedulerService;

namespace Infrastructure.Services.SchedulerService
{
    public class AuctionSchedulerService : IAuctionSchedulerService
    {
        private readonly ITimeTaskClient _timeTaskClient;
        private readonly TimeTaskServiceSettings _serviceSettings;

        public AuctionSchedulerService(ITimeTaskClient timeTaskClient, TimeTaskServiceSettings serviceSettings)
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
                AuctionId = auction.AggregateId
            };
            return sheduleRequest;
        }

        public Task<ScheduledTaskId> ScheduleAuctionEndTask(Auction auction)
        {
            var sheduleRequest = CreateScheduleRequest(auction);
            var infoResponse = _timeTaskClient.ScheduleTask(sheduleRequest).Result;

            return Task.FromResult(new ScheduledTaskId(infoResponse.Id));
        }

        public Task CancelAuctionEndTask(ScheduledTaskId id)
        {
            return _timeTaskClient.CancelTask("echo", id.Value);
        }
    }
}
