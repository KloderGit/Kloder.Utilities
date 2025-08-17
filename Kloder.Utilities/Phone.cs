using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Utilities;

[DebuggerDisplay("{_value}")]
[JsonConverter(typeof(PhoneJsonConverter))]
public partial class Phone : IEquatable<Phone>, IEquatable<string>
{
    private readonly string _value;
    private readonly string _digits;

    protected Phone() {}

    public Phone(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var trimmed = value.Trim();
        if (!IsValidPhoneNumber(trimmed))
            throw new ArgumentException("Invalid phone number", nameof(value));

        _digits = GetJustDigits(trimmed);

        // Спец-правило РФ: 8XXXXXXXXXX (11 цифр) -> +7XXXXXXXXXX
        if (_digits.Length == 11 && _digits[0] == '8') _value = "+7" + _digits[1..];
        else _value = "+" + _digits;
    }

    public bool Equals(Phone? other) =>
        other is not null && string.Equals(_digits, other._digits, StringComparison.Ordinal);

    public bool Equals(string? other) =>
        other is not null && string.Equals(_digits, GetJustDigits(other), StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj switch
    {
        Phone p  => Equals(p),
        string s => Equals(s),
        _        => false
    };
    
    public override int GetHashCode() => _digits.GetHashCode(StringComparison.Ordinal);
    
    public override string ToString() => _value;


    public static implicit operator string? (Phone? x) => x?._value;
    public static explicit operator Phone? (string? x) => TryParse(x, out var p) ? p : null;
    
    
    public static bool operator ==(Phone? left, Phone? right) => Equals(left, right);
    public static bool operator !=(Phone? left, Phone? right) => !Equals(left, right);

    public static bool operator ==(Phone? left, string? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Phone? left, string? right) => !(left == right);

    public static bool operator ==(string? left, Phone? right) => right?.Equals(left) ?? left is null;
    public static bool operator !=(string? left, Phone? right) => !(left == right);

    
    public static bool TryParse(string? value, out Phone? phone)
    {
        phone = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!IsValidPhoneNumber(value)) return false;

        try
        {
            phone = new Phone(value);
            return true;
        }
        catch
        {
            phone = null;
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber) => PhoneNumberRegex().IsMatch(phoneNumber);


    private static string GetJustDigits(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var arr = input.Where(char.IsDigit).ToArray();
        return new string(arr);
    }
    
    
    [GeneratedRegex(@"^(\+?\d{1,3}\s?)?\(?\d{1,3}\)?[\s-]?\d{1,4}[\s-]?\d{1,2}[\s-]?\d{1,2}$")]
    private static partial Regex PhoneNumberRegex();
}


public class PhoneJsonConverter : JsonConverter<Phone?>
{
    public override Phone? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var s = reader.GetString();
        return Phone.TryParse(s, out var phone) ? phone : null;
    }

    public override void Write(Utf8JsonWriter writer, Phone? phone, JsonSerializerOptions options)
    {
        if (phone is null) writer.WriteNullValue();
        else writer.WriteStringValue(phone.ToString());
    }
}