using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReadModel.Core.Model
{
    public class DecimalRoundingConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            var rounded = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
            writer.WriteStringValue(rounded.ToString(rounded % 1 == 0 ? "F0" : "F2"));
        }
    }
}