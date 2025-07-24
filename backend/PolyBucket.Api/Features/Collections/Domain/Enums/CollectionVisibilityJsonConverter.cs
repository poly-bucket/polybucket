using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyBucket.Api.Features.Collections.Domain.Enums
{
    public class CollectionVisibilityJsonConverter : JsonConverter<CollectionVisibility>
    {
        public override CollectionVisibility Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (Enum.TryParse<CollectionVisibility>(stringValue, true, out var result))
                {
                    return result;
                }
            }
            
            throw new JsonException($"Unable to convert '{reader.GetString()}' to CollectionVisibility.");
        }

        public override void Write(Utf8JsonWriter writer, CollectionVisibility value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class NullableCollectionVisibilityJsonConverter : JsonConverter<CollectionVisibility?>
    {
        public override CollectionVisibility? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }
                
                if (Enum.TryParse<CollectionVisibility>(stringValue, true, out var result))
                {
                    return result;
                }
            }
            
            throw new JsonException($"Unable to convert '{reader.GetString()}' to CollectionVisibility?.");
        }

        public override void Write(Utf8JsonWriter writer, CollectionVisibility? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
} 