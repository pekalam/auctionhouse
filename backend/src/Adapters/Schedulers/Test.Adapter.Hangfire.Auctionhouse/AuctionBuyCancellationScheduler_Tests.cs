using Adapter.Hangfire_.Auctionhouse;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.Services.ServiceContracts;
using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
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
    public class AuctionBuyCancellationScheduler_Tests : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;

        public AuctionBuyCancellationScheduler_Tests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Schedules_buy_cancellation_task_and_cancels_an_auction()
        {
            var auction = await GivenLockedAuction();
            var auctionRepositoryMock = GivenAuctionRepositoryMock(auction);
            var scheduler = SetupWebHostAndServices(auctionRepositoryMock, out var cts);
            
            scheduler.ScheduleAuctionBuyCancellation(auction.AggregateId, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(1)));

            await AssertThatAuctionBuyIsCancelled(auction);
            cts.Cancel();
        }

        [Fact]
        public async Task Cancels_scheduled_buy_cancellation_task()
        {
            var auction = await GivenLockedAuction();
            var auctionRepositoryMock = GivenAuctionRepositoryMock(auction);
            var scheduler = SetupWebHostAndServices(auctionRepositoryMock, out var cts);

            scheduler.ScheduleAuctionBuyCancellation(auction.AggregateId, TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(1)));
            scheduler.Cancel(auction.AggregateId);

            await AssertThatAuctionBuyIsNotCancelled(auction);
            cts.Cancel();
        }

        private static async Task AssertThatAuctionBuyIsCancelled(Auction auction)
        {
            await Task.Delay(20_000);
            Assert.Equal(UserId.Empty, auction.Buyer);
        }

        private static async Task AssertThatAuctionBuyIsNotCancelled(Auction auction)
        {
            await Task.Delay(20_000);
            Assert.NotEqual(UserId.Empty, auction.Buyer);
        }

        private IAuctionBuyCancellationScheduler SetupWebHostAndServices(Mock<IAuctionRepository> auctionRepositoryMock, out CancellationTokenSource cts)
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

                    new AuctionsDomainInstaller(services)
                        .AddHangfireAuctionBuyCancellationSchedulerAdapter(
                                connectionString: TestConfig.Instance.GetHangfireDbConnectionString(),
                                auctionRepositoryFactory: s => auctionRepositoryMock.Object,
                                eventOutboxFactory: s => Mock.Of<IEventOutbox>(),
                                unitOfWorkFactory: s => UnitOfWorkFactoryMock.Instance.Object
                            );
                })
                .Build();
            HangfireAuctionBuyCancellationSchedulerInstaller.Initialize(host.Services);

            cts = new CancellationTokenSource();
            var hostRunTask = host.RunAsync(cts.Token);
            return host.Services.GetRequiredService<IAuctionBuyCancellationScheduler>();
        }

        private static Mock<IAuctionRepository> GivenAuctionRepositoryMock(Auction auction)
        {
            var auctionRepositoryMock = new Mock<IAuctionRepository>();
            auctionRepositoryMock
                .Setup(f => f.FindAuction(It.Is<Guid>(i => i == auction.AggregateId.Value)))
                .Returns(auction);
            return auctionRepositoryMock;
        }

        private async static Task<Auction> GivenLockedAuction()
        {
            var auction = new GivenAuction().Build();
            var buyerId = UserId.New();
            var paymentVerificationScenario = AuctionPaymentVerificationContracts.SuccessfulScenario(auction, buyerId);
            var stubPaymentVerification = new GivenAuctionPaymentVerification().Build(paymentVerificationScenario);

            await auction.Buy(buyerId, paymentVerificationScenario.Given.paymentMethod, stubPaymentVerification, Mock.Of<IAuctionBuyCancellationScheduler>());

            // ensure locked
            Assert.Equal(buyerId, auction.Buyer);
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