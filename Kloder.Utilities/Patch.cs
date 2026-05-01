using System;
using System.Collections.Generic;
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

    public Patch(T? value)
    {
        HasValue = true;
        Value = value;
    }

    public void Apply(Action<T?> action)
    {
        if (HasValue) action(Value);
    }
    
    public TR Apply<TR>(Func<T?, TR> action, TR defaultValue)
        => !HasValue ? defaultValue : action(Value);
}


public readonly record struct CollectionPatch<T>
{
    /// <summary>Новые элементы для добавления</summary>
    public List<T>? Added { get; init; }
    
    /// <summary>ID элементов для удаления</summary>
    public List<Guid>? RemovedIds { get; init; }
    
    /// <summary>Элементы для обновления (должны содержать Id)</summary>
    public List<T>? Updated { get; init; }
    
    public bool HasAdded => Added is { Count: > 0 };
    public bool HasRemoved => RemovedIds is { Count: > 0 };
    public bool HasUpdated => Updated is { Count: > 0 };
    public bool IsEmpty => !HasAdded && !HasRemoved && !HasUpdated;
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