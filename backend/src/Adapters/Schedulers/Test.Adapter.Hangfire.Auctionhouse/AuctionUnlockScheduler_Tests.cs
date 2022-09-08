using Adapter.Hangfire_.Auctionhouse;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Tests.Base.Mocks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TestConfigurationAccessor;
using Xunit;
using Xunit.Abstractions;

namespace Test.Adapter.Hangfire.Auctionhouse
{
    [Trait("Category", "Integration")]
    public class AuctionUnlockScheduler_Tests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;

        public AuctionUnlockScheduler_Tests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Schedules_unlock_task_and_unlocks_auction()
        {
            var auction = GivenLockedAuction();
            var auctionRepositoryMock = GivenAuctionRepositoryMock(auction);
            var scheduler = SetupWebHostAndServices(auctionRepositoryMock, out var cts);
            
            scheduler.ScheduleAuctionUnlock(auction.AggregateId, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(1)));

            await AssertThatAuctionIsUnlocked(auction);
            cts.Cancel();
        }

        [Fact]
        public async Task Cancels_scheduled_unlock_task()
        {
            var auction = GivenLockedAuction();
            var auctionRepositoryMock = GivenAuctionRepositoryMock(auction);
            var scheduler = SetupWebHostAndServices(auctionRepositoryMock, out var cts);

            scheduler.ScheduleAuctionUnlock(auction.AggregateId, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(1)));
            scheduler.Cancel(auction.AggregateId);

            await AssertThatAuctionIsLocked(auction);
            cts.Cancel();
        }

        private static async Task AssertThatAuctionIsUnlocked(Auction auction)
        {
            await Task.Delay(20_000);
            Assert.False(auction.Locked);
        }

        private static async Task AssertThatAuctionIsLocked(Auction auction)
        {
            await Task.Delay(20_000);
            Assert.True(auction.Locked);
        }

        private IAuctionUnlockScheduler SetupWebHostAndServices(Mock<IAuctionRepository> auctionRepositoryMock, out CancellationTokenSource cts)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(c =>
                    {
                        c.AddXUnit(_outputHelper);
                        c.SetMinimumLevel(LogLevel.Debug);
                        //TODO hangfire logger based on _outputHelper
                    });

                    services.AddTransient<AuctionUnlockService>();
                    services.AddHangfireAuctionUnlockSchedulerAdapter(connectionString: TestConfig.Instance.GetHangfireDbConnectionString(),
                        auctionRepositoryFactory: s => auctionRepositoryMock.Object,
                        eventOutboxFactory: s => Mock.Of<IEventOutbox>(),
                        unitOfWorkFactory: s => UnitOfWorkFactoryMock.Instance.Object);
                })
                .Build();
            HangfireAdapterInstaller.Initialize(host.Services);

            cts = new CancellationTokenSource();
            var hostRunTask = host.RunAsync(cts.Token);
            return host.Services.GetRequiredService<IAuctionUnlockScheduler>();
        }

        private static Mock<IAuctionRepository> GivenAuctionRepositoryMock(Auction auction)
        {
            var auctionRepositoryMock = new Mock<IAuctionRepository>();
            auctionRepositoryMock
                .Setup(f => f.FindAuction(It.Is<Guid>(i => i == auction.AggregateId.Value)))
                .Returns(auction);
            return auctionRepositoryMock;
        }

        private static Auction GivenLockedAuction()
        {
            var auction = new GivenAuction().Build();
            auction.MarkPendingEventsAsHandled();
            // ensure locked
            Assert.True(auction.Locked);
            return auction;
        }

        public void Dispose()
        {
            var jobIds = new List<string>();

            using (var connection = new SqlConnection(TestConfig.Instance.GetHangfireDbConnectionString()))
            {
                connection.Open();

                var cmd = new SqlCommand("SELECT Id FROM Hangfire.Job", connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    jobIds.Add(reader.GetInt64(0).ToString());
                }
            }
            foreach (var jobId in jobIds)
            {
                BackgroundJob.Delete(jobId);
            }
        }
    }
}