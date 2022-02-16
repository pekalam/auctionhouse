using Core.Common.Domain;
using Core.DomainFramework;
using Newtonsoft.Json;

namespace Adapter.SqlServer.EventOutbox
{

    internal static class SerializationUtils
    {
        public static string ToJson(Event @event)
        {
            return JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.DateTime
            });
        }

        public static Event FromJson(string json) => JsonConvert.DeserializeObject<Event>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                    DateParseHandling = DateParseHandling.DateTime
                }) ?? throw new InfrastructureException("Could not parse event from json: " + json);
    }
}