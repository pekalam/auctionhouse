using Polly;
using System;
using System.Linq;
using Xunit;

namespace FunctionalTests.Commands
{
    public partial class TestBase
    {
        protected void AssertEventual(Func<bool> getResults)
        {
            var policy = Policy
              .HandleResult<bool>(p => p == false)
              .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
              );
            Assert.True(policy.Execute(getResults));
        }

        public bool ExpectedEventsShouldBePublished(Type[] expectedEvents)
        {
            var allEventsPublished = SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents).Any() == false;

            if (SentEvents.Count > expectedEvents.Length)
            {
                _outputHelper.WriteLine("Not all events were included in expected");
                foreach (var ev in SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents))
                {
                    _outputHelper.WriteLine("Event: " + ev.Name);
                }
            }
            else if (!allEventsPublished)
            {
                var notPublished = expectedEvents.Except(SentEvents.Select(e => e.Event.GetType()));
                _outputHelper.WriteLine($"Not all expected events were published ({notPublished.Count()}/{expectedEvents.Length}):");
                foreach (var ev in notPublished)
                {
                    _outputHelper.WriteLine("Not published: " + ev.Name);
                }
            }
            else
            {
                _outputHelper.WriteLine("All events were published");
            }

            return allEventsPublished;
        }
    }
}
