using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ.AutoSubscribe;

namespace FunctionalTests.EventHandling
{
    class FakeAutoSubscriberMessageDispatcher : IAutoSubscriberMessageDispatcher
    {
        private Dictionary<Type, object> _consumers = new Dictionary<Type, object>();

        public FakeAutoSubscriberMessageDispatcher()
        {
        }

        public void Setup<TMessage, TConsumer>(TConsumer consumer) where TMessage : class
            where TConsumer : class, IConsume<TMessage>
        {
            _consumers.Add(typeof(TConsumer), consumer);
        }

        public void SetupAsync<TMessage, TConsumer>(TConsumer consumer) where TMessage : class
            where TConsumer : class, IConsumeAsync<TMessage>
        {
            _consumers.Add(typeof(TConsumer), consumer);
        }

        public void Dispatch<TMessage, TConsumer>(TMessage message) where TMessage : class
            where TConsumer : class, IConsume<TMessage>
        {
            var o = (_consumers[typeof(TConsumer)] as TConsumer);
            o.Consume(message);
        }

        public async Task DispatchAsync<TMessage, TConsumer>(TMessage message) where TMessage : class
            where TConsumer : class, IConsumeAsync<TMessage>
        {
            await (_consumers[typeof(TConsumer)] as TConsumer).ConsumeAsync(message);
        }
    }
}