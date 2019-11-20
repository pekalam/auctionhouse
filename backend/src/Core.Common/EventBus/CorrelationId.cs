using System;

namespace Core.Common.EventBus
{
    public class CorrelationId
    {
        public string Value { get; set; }

        public CorrelationId(string value)
        {
            Value = value;
        }

        public static implicit operator CorrelationId(string value) => new CorrelationId(value);
    }
}