using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Adapter.Hangfire_.Auctionhouse
{
    internal interface IAuctionCancelSchedulerJobIdFinder
    {
        Task<string?> FindJobId(AuctionId auctionId);
    }

    internal class AuctionCancelSchedulerJobIdFinder : IAuctionCancelSchedulerJobIdFinder
    {
        private readonly SqlServerSettings _sqlServerSettings;

        public AuctionCancelSchedulerJobIdFinder(SqlServerSettings sqlServerSettings)
        {
            _sqlServerSettings = sqlServerSettings;
        }

        private static string GetAuctionIdColumnValueRepresentation(AuctionId auctionId)
        {
            return $"[\"{{\\\"Value\\\":\\\"{auctionId.Value}\\\"}}\"]";
        }

        public async Task<string?> FindJobId(AuctionId auctionId)
        {
            var argumentsColValue = GetAuctionIdColumnValueRepresentation(auctionId);

            using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);
            await connection.OpenAsync();

            var cmd = new SqlCommand("SELECT Id FROM Hangfire.Job WHERE Arguments = @Arguments", connection);
            cmd.Parameters.AddWithValue("@Arguments", argumentsColValue);

            var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return null;
            }
            var jobId = reader.GetInt64(0).ToString();
            return jobId;
        }
    }


    internal class AuctionBuyCancellationScheduler : IAuctionBuyCancellationScheduler
    {
        public const int MaxRetries = 6;

        private readonly AuctionBuyCancellationService _auctionBuyCancellationService;
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<AuctionBuyCancellationScheduler> _logger;
        private readonly IEventOutbox _eventOutbox;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IAuctionCancelSchedulerJobIdFinder _jobIdFinder;

        public AuctionBuyCancellationScheduler(AuctionBuyCancellationService auctionBuyCancellationService, IAuctionRepository auctions, ILogger<AuctionBuyCancellationScheduler> logger,
            IEventOutbox eventOutbox, IUnitOfWorkFactory uowFactory, IAuctionCancelSchedulerJobIdFinder jobIdFinder)
        {
            _auctionBuyCancellationService = auctionBuyCancellationService;
            _auctions = auctions;
            _logger = logger;
            _eventOutbox = eventOutbox;
            _uowFactory = uowFactory;
            _jobIdFinder = jobIdFinder;
        }

        public void Cancel(AuctionId auctionId)
        {
            var jobId = _jobIdFinder.FindJobId(auctionId).GetAwaiter().GetResult();
            BackgroundJob.Delete(jobId);
        }

        [AutomaticRetry(Attempts = MaxRetries, DelaysInSeconds = new[] {10,10,10,10,10,10},  OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task CancelAuctionBuyAsync(AuctionId auctionId)
        {
            var cancelledAuction = _auctionBuyCancellationService.Cancel(auctionId, _auctions);
            if (cancelledAuction is null)
            {
                _logger.LogWarning("Could not find auction {auctionId} for buy cancellation", auctionId);
                return;
            }
            if (cancelledAuction.PendingEvents.Count == 0)
            {
                _logger.LogWarning("Could not cancel auction {auctionId}", auctionId);
                return;
            }

            _logger.LogDebug("Cancelling auction buy of auction {auctionId}", auctionId);
            using (var uow = _uowFactory.Begin())
            {
                // there is no need to try to send events faster like in CommandHandlerBase because this event has not strict timing requirements
                _auctions.UpdateAuction(cancelledAuction);
                await _eventOutbox.SaveEvents(cancelledAuction.PendingEvents,
                    CommandContext.CreateNew(nameof(AuctionBuyCancellationScheduler)));
                uow.Commit();
            }
            cancelledAuction.MarkPendingEventsAsHandled();
        }

        public void ScheduleAuctionBuyCancellation(AuctionId auctionId, TimeOnly time)
        {
            BackgroundJob.Schedule<AuctionBuyCancellationScheduler>((scheduler) => scheduler.CancelAuctionBuyAsync(auctionId), time.ToTimeSpan());
        }
    }
}