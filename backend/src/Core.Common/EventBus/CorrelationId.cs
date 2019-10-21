namespace Core.Common.EventBus
{
    public class CorrelationId
    {
        public string Value { get; }

        public CorrelationId(string value)
        {
            Value = value;
        }
    }
}