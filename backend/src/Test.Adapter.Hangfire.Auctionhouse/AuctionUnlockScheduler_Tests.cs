using Adapter.Hangfire_.Auctionhouse;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Events;
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
using Test.Auctions.Base.Builders;
using Test.Common.Base.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Test.Adapter.Hangfire.Auctionhouse
{
    [Trait("Category", "Integration")]
    public class AuctionUnlockScheduler_Tests : IDisposable
    {
        private const string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Marek\source\repos\Csharp\auctionhouse\backend\src\Tests\FunctionalTestsServer.mdf;Integrated Security=True;Connect Timeout=30";
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

                    services.AddTransient<IAuctionRepository>(s => auctionRepositoryMock.Object);
                    services.AddTransient<IEventOutbox>(s => Mock.Of<IEventOutbox>());
                    services.AddTransient<IUnitOfWorkFactory>(s => UnitOfWorkFactoryMock.Instance.Object);
                    services.AddTransient<AuctionUnlockService>();
                    services.AddHangfireServices(ConnectionString);
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

            using (var connection = new SqlConnection(ConnectionString))
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