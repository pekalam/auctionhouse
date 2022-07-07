namespace RabbitMq.EventBus
{
    public class RabbitMqSettings
    {
        /// <summary>
        /// Connection string that can be used for testing purposes
        /// </summary>
        public const string SampleRabbitMqConnectionString = "host=localhost;publisherConfirms=true;persistentMessages=true;prefetchcount=0";

        public string ConnectionString { get; set; } = null!;

        internal void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentException(nameof(ConnectionString) + " must not be empty");
            }

            if (!ConnectionString.Contains("publisherConfirms=true"))
            {
                throw new ArgumentException(nameof(ConnectionString) + " must have publisherConfirms enabled");
            }

            if (!ConnectionString.Contains("persistentMessages=true"))
            {
                throw new ArgumentException(nameof(ConnectionString) + " must have set persistentMessages=true");
            }

            if (!ConnectionString.Contains("prefetchcount"))
            {
                throw new ArgumentException(nameof(ConnectionString) + " must specify prefetchcount");
            }
        }
    }
}