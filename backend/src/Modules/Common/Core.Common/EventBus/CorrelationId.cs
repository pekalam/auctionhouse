using System;

namespace Core.Common.EventBus
{
    public class CorrelationId
    {
        public string Value { get; set; }

        public CorrelationId(string value) => Value = value;

        public static CorrelationId CreateNew() => new CorrelationId(Guid.NewGuid().ToString());

        public static implicit operator CorrelationId(string value) => new CorrelationId(value);
    }
}