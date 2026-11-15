using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Fix;

public class Json
{
    //
    // Options for reading and writing session files. Only properties marked [JsonInclude] are
    // stored, enumerations and FIX versions are stored by name, and property names are matched
    // ignoring case, as Newtonsoft.Json did when it wrote the files.
    //
    public static JsonSerializerOptions SessionOptions { get; } = CreateSessionOptions();

    static JsonSerializerOptions CreateSessionOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Write characters such as accented letters as they are rather than as \u escapes.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { IncludeOnlyMarkedProperties }
            }
        };

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new FixVersionConverter());
        options.MakeReadOnly();

        return options;
    }

    static void IncludeOnlyMarkedProperties(JsonTypeInfo info)
    {
        if (info.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        for (int index = info.Properties.Count - 1; index >= 0; --index)
        {
            if (info.Properties[index].AttributeProvider?.IsDefined(typeof(JsonIncludeAttribute), inherit: true) != true)
            {
                info.Properties.RemoveAt(index);
            }
        }
    }

    public class FixVersionConverter : JsonConverter<Dictionary.Version>
    {
        readonly List<Dictionary.Version> _versions = new();

        public FixVersionConverter()
        {
            foreach (var version in Dictionary.Versions)
            {
                if (version.BeginString.StartsWith("FIX.5.0", StringComparison.Ordinal))
                    continue;
                _versions.Add(version);
            }
        }

        // Returns null for a version this converter doesn't read, including the FIX.5.0 versions,
        // which can't be a session's BeginString. Session files leave the property unchanged then.
        public override Dictionary.Version? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing Fix.Dictionary.Version.");
            }

            string? value = reader.GetString();

            return _versions.FirstOrDefault(version => version.BeginString == value);
        }

        public override void Write(Utf8JsonWriter writer, Dictionary.Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.BeginString);
        }
    }
}
