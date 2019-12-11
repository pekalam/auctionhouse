using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Web;
using Web.Auth;
using Web.Dto.Commands;

namespace Tests
{
    public static class TestUtils
    {
        public const string TestUserGuid = "ba28efaa-6b68-4c52-b4bb-0fe7e264a632";
        public const string Test2UserGuid = "6ab0dc7f-09fa-4fd4-b3a0-261a6d5e4878";
        public const string TestAuctionGuid = "d1d68597-8eb8-4e71-ad42-87ebb272781b";

        public static HttpRequestMessage AddUserAuthHeader(this HttpRequestMessage req,
            WebApplicationFactory<Startup> _factory)
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();
                var testUserJwt = jwtService.IssueToken(Guid.Parse(TestUserGuid), "test");
                req.Headers.Add("Authorization", $"Bearer {testUserJwt}");
                return req;
            }
        }

        public static HttpRequestMessage AddJsonBody(this HttpRequestMessage req, object obj)
        {
            req.Content =
                new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            return req;
        }

        public static async Task<RequestStatusDto> GetRequestStatus(this HttpResponseMessage res)
        {
            var responseStr = await res.Content.ReadAsStringAsync();
            var requestStatus = JsonConvert.DeserializeObject<RequestStatusDto>(responseStr);
            return requestStatus;
        }

        public static HttpRequestMessage AddUserAuthHeader(this HttpRequestMessage req, WebApplicationFactory<Startup> _factory, 
            string username, string userId)
        {
            using (var scope = _factory.Server.Host.Services.CreateScope())
            {
                var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();
                var testUserJwt = jwtService.IssueToken(Guid.Parse(userId), username);
                req.Headers.Add("Authorization", $"Bearer {testUserJwt}");
                return req;
            }
        }
    }

    public class Tests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient client;
        private Mock<IRequestStatusService> mockRequestStatusService;

        [SetUp]
        public void Setup()
        {
            mockRequestStatusService = new Mock<IRequestStatusService>();
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(collection =>
                    {
                        collection.AddScoped<IRequestStatusService>(provider =>
                            mockRequestStatusService.Object);
                    });
                });

            client = _factory.CreateClient();
        }

        [Test]
        public async Task ApiBidCommand_when_issued_by_creator_fails()
        {
            var fixture = new Fixture();
            BidCommandDto commandDto = fixture.Build<BidCommandDto>()
                .With(dto => dto.AuctionId, TestUtils.TestAuctionGuid)
                .With(dto => dto.Price, 20)
                .Create();

            var sem = new SemaphoreSlim(0, 1);
            mockRequestStatusService.Setup(service => service.TrySendRequestFailureToUser(It.IsAny<string>(),
                    It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(), null))
                .Callback(() => sem.Release());
            mockRequestStatusService.Verify(service => service.TrySendRequestCompletionToUser(It.IsAny<string>(),
                It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(), null), Times.Never());


            var cmdReq = new HttpRequestMessage(HttpMethod.Post, "/api/bid")
                .AddUserAuthHeader(_factory)
                .AddJsonBody(commandDto);

            var response = await client.SendAsync(cmdReq);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var requestStatus = await response.GetRequestStatus();
            requestStatus.Status.Should().Be("PENDING");

            if (!sem.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail();
            }
        }


        [Test]
        public async Task ApiBidCommand_when_issued_not_by_creator_completes()
        {
            var fixture = new Fixture();
            BidCommandDto commandDto = fixture.Build<BidCommandDto>()
                .With(dto => dto.AuctionId, TestUtils.TestAuctionGuid)
                .With(dto => dto.Price, 20)
                .Create();

            var sem = new SemaphoreSlim(0, 1);
            mockRequestStatusService.Verify(service => service.TrySendRequestFailureToUser(It.IsAny<string>(),
                It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(), null), Times.Never());
            mockRequestStatusService.Setup(service => service.TrySendRequestCompletionToUser(It.IsAny<string>(),
                    It.IsAny<CorrelationId>(), It.IsAny<UserIdentity>(), null))
                .Callback(() => sem.Release());


            var cmdReq = new HttpRequestMessage(HttpMethod.Post, "/api/bid")
                .AddUserAuthHeader(_factory, "test2", TestUtils.Test2UserGuid)
                .AddJsonBody(commandDto);

            var response = await client.SendAsync(cmdReq);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var requestStatus = await response.GetRequestStatus();
            requestStatus.Status.Should().Be("PENDING");

            if (!sem.Wait(TimeSpan.FromSeconds(10)))
            {
                Assert.Fail();
            }
        }

        [Test]
        public async Task ApiCreateAuction_when_session_started_creates_auction()
        {
            var fixture = new Fixture();

            var categoryTreeService = new CategoryTreeService(new CategoryNameServiceSettings()
                {CategoriesFilePath = "categories.xml"});
            categoryTreeService.Init();
            List<string> categories = new List<string>()
            {
                categoryTreeService.GetCategoriesTree().SubCategories[0].CategoryName,
                categoryTreeService.GetCategoriesTree().SubCategories[0].SubCategories[1].CategoryName,
                categoryTreeService.GetCategoriesTree().SubCategories[0].SubCategories[1].SubCategories[2].CategoryName,
            };


            CreateAuctionCommandDto createAuctionCmd = fixture.Build<CreateAuctionCommandDto>()
                .With(dto => dto.Category, categories)
                .With(dto => dto.StartDate, DateTime.UtcNow)
                .With(dto => dto.EndDate, DateTime.UtcNow.AddMonths(1))
                .With(dto => dto.Tags, new[] {"tag1", "tag2"})
                .Create();

            var startSessionReq = new HttpRequestMessage(HttpMethod.Post, "/api/startCreateSession")
                .AddUserAuthHeader(_factory);
            var startSessionResponse = await client.SendAsync(startSessionReq);

            startSessionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var req = new HttpRequestMessage(HttpMethod.Post, "/api/createAuction")
                .AddUserAuthHeader(_factory)
                .AddJsonBody(createAuctionCmd);
            var response = await client.SendAsync(req);


            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var requestStatus = await response.GetRequestStatus();
            requestStatus.CorrelationId.Should().NotBeEmpty();
            requestStatus.Status.Should().Be("COMPLETED");
        }
    }
}