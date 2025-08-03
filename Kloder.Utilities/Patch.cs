using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;

[DebuggerDisplay("{Value ?? \"null\"}")]
[JsonConverter(typeof(PatchJsonConverterFactory))]
public readonly record struct Patch<T>
{
    public bool HasValue { get; init; }
    public T? Value { get; init; }

    public Patch()
    {
        HasValue = false;
        Value = default;
    }

    public Patch(T? value)
    {
        HasValue = true;
        Value = value;
    }
}

public class PatchJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Patch<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PatchJsonConverter<>).MakeGenericType(innerType);
        
        var result = (JsonConverter)Activator.CreateInstance(converterType)!;

        return result;
    }
}


public class PatchJsonConverter<T> : JsonConverter<Patch<T>>
{
    public override Patch<T> Read(ref Utf8JsonReader reader, Type? typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new Patch<T>(default);

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Patch<T>(value);
    }

    public override void Write(Utf8JsonWriter writer, Patch<T> value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}