using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Utilities;

[DebuggerDisplay("{_value}")]
[JsonConverter(typeof(EmailJsonConverter))]
public partial class Email : IEquatable<Email>, IEquatable<string>
{
    private readonly string _value;

    protected Email() {}

    public Email(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (!IsValidEmail(normalized))
            throw new ArgumentException("Invalid email address", nameof(value));

        _value = normalized;
    }

    
    public override string ToString() => _value;
    
    
    public bool Equals(Email? other) =>
        other is not null &&
        string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
    
    
    public bool Equals(string? other) =>
        other is not null &&
        string.Equals(_value, other.Trim(), StringComparison.OrdinalIgnoreCase);
    
    
    public override bool Equals(object? obj) => obj switch
    {
        Email e   => Equals(e),
        string s  => Equals(s),
        _         => false
    };

    
    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(_value);
    
    
    public static implicit operator string? (Email? x) => x?._value;
    public static explicit operator Email? (string? x) => TryParse(x, out var e) ? e : null;
    
    
    public static bool operator ==(Email? left, Email? right) => Equals(left, right);
    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

    public static bool operator ==(Email? left, string? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Email? left, string? right) => !(left == right);

    public static bool operator ==(string? left, Email? right) => right?.Equals(left) ?? left is null;
    public static bool operator !=(string? left, Email? right) => !(left == right);
    
    
    private static bool IsValidEmail(string email) => EmailTemplateRegExp().IsMatch(email);
    
    
    public static bool TryParse(string? value, out Email? email)
    {
        email = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = value.Trim().ToLowerInvariant();
        if (!IsValidEmail(normalized)) return false;

        email = new Email(normalized);
        return true;
    }
    
    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex EmailTemplateRegExp();
}


public class EmailJsonConverter : JsonConverter<Email?>
{
    public override Email? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var s = reader.GetString();
        return Email.TryParse(s, out var email) ? email : null;
    }

    public override void Write(Utf8JsonWriter writer, Email? email, JsonSerializerOptions options)
    {
        if (email is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(email.ToString());
    }
}