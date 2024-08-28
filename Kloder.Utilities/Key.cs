using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;

[DebuggerDisplay("{_value}")]
[JsonConverter(typeof(KeyJsonConverter))]
public sealed class Key<T> : IEquatable<Key<T>>
{
    private readonly Guid _value;

    public Key() => _value = Guid.NewGuid();
    public Key(Guid key) => _value = key;

    public override string ToString() => _value.ToString();

    public bool Equals(Key<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || _value.Equals(other._value);
    }
    
    public override bool Equals(object? obj) 
        => ReferenceEquals(this, obj) || obj is Key<T> other && Equals(other);
    
    private bool Equals(Guid other) => _value.Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(Key<T> a, Key<T> b) => a._value.Equals(b._value);
    public static bool operator !=(Key<T> a, Key<T> b) => !a._value.Equals(b._value);
    public static bool operator ==(Key<T> a, Guid b) => a.Equals(b);
    public static bool operator !=(Key<T> a, Guid b) => !a.Equals(b);
    public static bool operator ==(Guid a, Key<T> b) => a.Equals(b);
    public static bool operator !=(Guid a, Key<T> b) => !a.Equals(b);

    public static implicit operator Guid(Key<T> x) => x._value;
    public static explicit operator Key<T>(Guid x) => new Key<T>(x);
}


public sealed class KeyJsonConverter : JsonConverterFactory
{
    private static readonly Dictionary<Type, JsonConverter> Converters = new();

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType 
           && typeToConvert.GetGenericTypeDefinition() == typeof(Key<>);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        var keyType = type.GetGenericArguments()[0];

        if (Converters.TryGetValue(type, out JsonConverter? existingConverter))
            return existingConverter;

        var converter = (JsonConverter)Activator
            .CreateInstance(typeof(KeyConverter<>).MakeGenericType(keyType))!;

        Converters[type] = converter;

        return converter;
    }

    private sealed class KeyConverter<T> : JsonConverter<Key<T>>
    {
        public override Key<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!;
            var guid = new Guid(value);
            return new Key<T>(guid);
        }

        public override void Write(Utf8JsonWriter writer, Key<T> value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}