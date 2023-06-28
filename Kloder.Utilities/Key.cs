using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;

[JsonConverter(typeof(KeyJsonConverter))]
[DebuggerDisplay("{Value}")]
public readonly struct Key<T> : IEquatable<Key<T>>
{
    private Guid Value { get; }

    public Key() => Value = Guid.NewGuid();
    public Key(Guid key) => Value = key;

    public override string ToString() => Value.ToString();
    
    public bool Equals(Key<T> other)
    {
        return this.Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            Key<T> key => Equals(key),
            Guid guid => Value.Equals(guid),
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public static bool operator ==(Key<T> a, Key<T> b) => a.Value.Equals(b.Value);
    public static bool operator !=(Key<T> a, Key<T> b) => !a.Value.Equals(b.Value);

    public static implicit operator Guid(Key<T> x)
    {
        return x.Value;
    }
    
    public static explicit operator Key<T>(Guid x)
    {
        return new Key<T>(x);
    }
}


public class KeyJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        if (typeToConvert.GetGenericTypeDefinition() != typeof(Key<>))
        {
            return false;
        }

        return true;
    }
    
    public override JsonConverter CreateConverter(
        Type type,
        JsonSerializerOptions options)
    {
        Type keyType = type.GetGenericArguments()[0];

        JsonConverter converter = (JsonConverter)Activator
            .CreateInstance(
            typeof(KeyConverter<>).MakeGenericType(
                new Type[] { keyType }))!;

        return converter;
    }

    private class KeyConverter<T> : JsonConverter<Key<T>>
    {
        public override Key<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!;
            var guid = new Guid(value);
            return new Key<T>(guid);
        }

        public override void Write(Utf8JsonWriter writer, Key<T> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}