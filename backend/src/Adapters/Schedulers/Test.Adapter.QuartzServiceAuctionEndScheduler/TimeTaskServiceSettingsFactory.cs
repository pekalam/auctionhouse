using Microsoft.Extensions.Configuration;
using QuartzTimeTaskService.AuctionEndScheduler;
using TestConfigurationAccessor;

namespace Test.Adapter.QuartzServiceAuctionEndScheduler
{
    public class TimeTaskServiceSettingsFactory
    {
        private const string SectionName = "QuartzAuctionEndSchedulerTests";

        public static string Url { get; set; }

        public static TimeTaskServiceSettings Create()
        {
            return new TimeTaskServiceSettings
            {
                ConnectionString = TestConfig.Instance.GetSection(SectionName)["ConnectionString"],
                ApiKey = TestConfig.Instance.GetSection(SectionName)["ApiKey"],
                AuctionEndEchoTaskEndpoint = Url.Replace("localhost", "host.docker.internal") + "/foo",
            };
        }
    }
}