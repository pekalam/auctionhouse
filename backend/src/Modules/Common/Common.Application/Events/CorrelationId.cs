using System.Diagnostics;

namespace Common.Application.Events
{
    public class CorrelationId
    {
        public string Value { get; set; }

        public CorrelationId(string value) => Value = value;

        public static CorrelationId CreateNew() => new CorrelationId(Activity.Current?.TraceId.ToString() ?? ActivityTraceId.CreateRandom().ToString());

        public static implicit operator CorrelationId(string value) => new CorrelationId(value);
    }
}