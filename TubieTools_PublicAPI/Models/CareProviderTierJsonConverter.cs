using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Custom JSON converter for CareProviderTier enum that handles both string names and integer values
    /// </summary>
    public class CareProviderTierJsonConverter : JsonConverter<CareProviderTier>
    {
        public override CareProviderTier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    {
                        var stringValue = reader.GetString();
                        if (Enum.TryParse<CareProviderTier>(stringValue, ignoreCase: true, out var result))
                        {
                            return result;
                        }
                        throw new JsonException($"Unable to convert \"{stringValue}\" to enum type \"{typeToConvert.Name}\".");
                    }

                case JsonTokenType.Number:
                    {
                        if (reader.TryGetInt32(out var intValue))
                        {
                            if (Enum.IsDefined(typeToConvert, intValue))
                            {
                                return (CareProviderTier)intValue;
                            }
                        }
                        throw new JsonException($"Unable to convert value to enum type \"{typeToConvert.Name}\".");
                    }

                default:
                    throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum \"{typeToConvert.Name}\".");
            }
        }

        public override void Write(Utf8JsonWriter writer, CareProviderTier value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
