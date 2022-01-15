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
    internal interface IAuctionUnlockSchedulerJobIdFinder
    {
        Task<string?> FindJobId(AuctionId auctionId);
    }

    internal class AuctionUnlockSchedulerJobIdFinder : IAuctionUnlockSchedulerJobIdFinder
    {
        private readonly SqlServerSettings _sqlServerSettings;

        public AuctionUnlockSchedulerJobIdFinder(SqlServerSettings sqlServerSettings)
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


    internal class AuctionUnlockScheduler : IAuctionUnlockScheduler
    {
        private readonly AuctionUnlockService _auctionUnlockService;
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<AuctionUnlockScheduler> _logger;
        private readonly IEventOutbox _eventOutbox;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IAuctionUnlockSchedulerJobIdFinder _jobIdFinder;

        public AuctionUnlockScheduler(AuctionUnlockService auctionUnlockService, IAuctionRepository auctions, ILogger<AuctionUnlockScheduler> logger,
            IEventOutbox eventOutbox, IUnitOfWorkFactory uowFactory, IAuctionUnlockSchedulerJobIdFinder jobIdFinder)
        {
            _auctionUnlockService = auctionUnlockService;
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

        public async Task UnlockAuctionAsync(AuctionId auctionId)
        {
            var unlockedAuction = _auctionUnlockService.Unlock(auctionId, _auctions);
            if (unlockedAuction is null)
            {
                _logger.LogWarning("Could not find auction {auctionId} to unlock", auctionId);
                return;
            }
            if (unlockedAuction.PendingEvents.Count == 0)
            {
                if (unlockedAuction.Completed) _logger.LogWarning("Auction {auctionId} was completed", auctionId);
                else _logger.LogWarning("Auction {auctionId} was not locked", auctionId);
                return;
            }

            _logger.LogDebug("Unlocking auction {auctionId}", auctionId);
            using (var uow = _uowFactory.Begin())
            {
                // there is no need to try to send events faster like in CommandHandlerBase because this unlocked event has not strict timing requirements
                _auctions.UpdateAuction(unlockedAuction);
                await _eventOutbox.SaveEvents(unlockedAuction.PendingEvents,
                    CommandContext.CreateNew(nameof(AuctionUnlockScheduler)),
                    ReadModelNotificationsMode.Disabled);
                uow.Commit();
            }
            unlockedAuction.MarkPendingEventsAsHandled();
        }

        public void ScheduleAuctionUnlock(AuctionId auctionId, TimeOnly time)
        {
            BackgroundJob.Schedule<AuctionUnlockScheduler>((unlockScheduler) => unlockScheduler.UnlockAuctionAsync(auctionId), time.ToTimeSpan());
        }
    }
}