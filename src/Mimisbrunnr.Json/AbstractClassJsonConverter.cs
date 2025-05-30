using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mimisbrunnr.Json;

internal class AbstractClassConverter<T> : JsonConverter<T>
{
    private readonly Type[] _assemblyTypes = typeof(T).Assembly.GetTypes();
    private readonly Type _baseType = typeof(T);

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("JsonTokenType.StartObject not found.");

        if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName
                           || reader.GetString() != "$type")
            throw new JsonException("Property $type not found.");

        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            throw new JsonException("Value at $type is invalid.");

        string typeName = reader.GetString();

        var type = _assemblyTypes.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        using (var output = new MemoryStream())
        {
            ReadObject(ref reader, output, options);
            return (T)JsonSerializer.Deserialize(output.ToArray(), type, options);
        }
    }

    private static void ReadObject(ref Utf8JsonReader reader, Stream output, JsonSerializerOptions options)
    {
        using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
        {
            Encoder = options.Encoder,
            Indented = options.WriteIndented
        });
        writer.WriteStartObject();
        var objectIntend = 0;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    objectIntend++;
                    break;
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    if (objectIntend == 0)
                    {
                        writer.Flush();
                        return;
                    }
                    objectIntend--;
                    break;
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;
                case JsonTokenType.PropertyName:
                    writer.WritePropertyName(reader.GetString());
                    break;
                case JsonTokenType.Comment:
                    writer.WriteCommentValue(reader.GetComment());
                    break;
                case JsonTokenType.String:
                    writer.WriteStringValue(reader.GetString());
                    break;
                case JsonTokenType.Number:
                    writer.WriteNumberValue(reader.GetInt32());
                    break;
                case JsonTokenType.True:
                case JsonTokenType.False:
                    writer.WriteBooleanValue(reader.GetBoolean());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var valueType = value.GetType();
        writer.WriteString("$type", valueType.Name);

        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using (var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            MaxDepth = options.MaxDepth
        }))
        {
            foreach (var jsonProperty in document.RootElement.EnumerateObject())
                jsonProperty.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAbstract && !typeof(IEnumerable).IsAssignableFrom(typeToConvert)
        && _baseType.IsAssignableFrom(typeToConvert);
}