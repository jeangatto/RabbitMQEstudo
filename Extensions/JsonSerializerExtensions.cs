using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitWebApp.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            AllowTrailingCommas = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        static JsonSerializerExtensions()
        {
            JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public static T FromJson<T>(this string jsonString)
            => JsonSerializer.Deserialize<T>(jsonString, JsonOptions);

        public static string ToJson<T>(this T value)
            => JsonSerializer.Serialize(value, JsonOptions);
    }
}