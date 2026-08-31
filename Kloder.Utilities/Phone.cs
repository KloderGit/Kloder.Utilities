using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Utilities;

[DebuggerDisplay("{Value}")]
[JsonConverter(typeof(PhoneJsonConverter))]
public partial class Phone : IEquatable<Phone>, IEquatable<string>
{
    protected readonly string Value;
    protected readonly string Digits;

    protected Phone() {}

    public Phone(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!TryGetValidDigits(value, out var digits))
            throw new ArgumentException("Invalid phone number", nameof(value));

        Digits = digits;
        Value  = "+" + digits;
    }

    public bool Equals(Phone? other) =>
        other is not null && string.Equals(Digits, other.Digits, StringComparison.Ordinal);

    public bool Equals(string? other) =>
        other is not null && string.Equals(Digits, NormalizeDigits(other), StringComparison.Ordinal);
    
    public override bool Equals(object? obj) => obj switch
    {
        Phone p  => Equals(p),
        string s => Equals(s),
        _        => false
    };
    
    private static string NormalizeDigits(string input) => NormalizeRawDigits(GetJustDigits(input));

    private static string NormalizeRawDigits(string digits)
    {
        if (digits.StartsWith("00")) digits = digits[2..];

        if (digits.Length == 11 && digits[0] == '8')
            digits = "7" + digits[1..];

        return digits;
    }
    
    public override int GetHashCode() => Digits.GetHashCode(StringComparison.Ordinal);
    
    public override string ToString() => Value;


    public static implicit operator string? (Phone? x) => x?.Value;
    public static explicit operator Phone? (string? x) => TryParse(x, out var p) ? p : null;
    
    
    public static bool operator ==(Phone? left, Phone? right) => Equals(left, right);
    public static bool operator !=(Phone? left, Phone? right) => !Equals(left, right);

    public static bool operator ==(Phone? left, string? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(Phone? left, string? right) => !(left == right);

    public static bool operator ==(string? left, Phone? right) => right?.Equals(left) ?? left is null;
    public static bool operator !=(string? left, Phone? right) => !(left == right);

    
    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && TryGetValidDigits(value, out _);

    public static bool TryParse(string? value, out Phone? phone)
    {
        phone = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

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

    private static bool TryGetValidDigits(string phoneNumber, out string digits)
    {
        digits = string.Empty;
        if (!PhoneNumberRegex().IsMatch(phoneNumber)) return false;

        var normalized = NormalizeDigits(phoneNumber);
        if (normalized.Length != 11 || normalized[0] != '7') return false;

        digits = normalized;
        return true;
    }


    private static string GetJustDigits(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var arr = input.Where(char.IsDigit).ToArray();
        return new string(arr);
    }


    [GeneratedRegex(@"^\+?[\d\s()-]+$")]
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