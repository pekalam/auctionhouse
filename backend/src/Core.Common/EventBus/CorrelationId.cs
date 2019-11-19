using System;

namespace Core.Common.EventBus
{
    public class CorrelationId
    {
        public string Value { get; }

        public CorrelationId(string value)
        {
            Value = value;
        }

        public CorrelationId()
        {
            Value = Guid.NewGuid().ToString();
        }

        public static implicit operator CorrelationId(string value) => new CorrelationId(value);
    }
}