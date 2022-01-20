using System;
using Newtonsoft.Json;

namespace ReadModel.Core.Model
{
    public class DecimalRoundingConverter : JsonConverter<decimal>
    {
        public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
        {
            var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
            writer.WriteValue(rounded);
        }

        public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => false;
    }
}