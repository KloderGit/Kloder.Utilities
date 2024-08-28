using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Utilities;

[DebuggerDisplay("{_value}")]
[JsonConverter(typeof(PhoneJsonConverter))]
public readonly partial struct Phone
{
    private readonly string _value = string.Empty;
    private readonly string _digits = string.Empty;

    public Phone(string value)
    {
        var phoneString = value.Trim();
        if (IsValidPhoneNumber(phoneString) == false) throw new ArgumentException("Invalid phone number", nameof(value));
        if (phoneString[0] == '8') phoneString = string.Concat("+7", phoneString.AsSpan(1));
        _value = phoneString;
        _digits = GetJustDigits(phoneString);
    }

    public bool Equals(Phone other) => _digits == other._digits;
    
    /// <summary>
    /// Важно, стараться не использовать в циклических операциях например - 'phonesArray.intersect(stringArray)'
    /// т.к для каждого string будет вызываться GetJustDigits.
    /// Лучше изменить порядок - stringArray.intersect(phonesArray) т.к приведение Phone в string не вызовет доп действий
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(string other) => _digits == GetJustDigits(other);
    
    public override bool Equals(object? obj)
    {
        var result = obj switch
        {
            Phone other => Equals(other),
            string other => Equals(other),
            _ => false
        };
        
        return result;
    }
    
    public override int GetHashCode() => _digits.GetHashCode();
    
    public override string ToString() => _value;

    
    private static bool IsValidPhoneNumber(string phoneNumber) => PhoneNumberRegex().IsMatch(phoneNumber);
    

    private static string GetJustDigits(string input)
    {
        var digits = input.Where(char.IsDigit).ToArray();
        return new string(digits);
    }

    public static implicit operator string(Phone x) => x._value;
    public static explicit operator Phone(string x) => new(x);
    
    
    public static bool operator ==(Phone left, Phone right) => left.Equals(right);
    public static bool operator !=(Phone left, Phone right) => !left.Equals(right);
    public static bool operator ==(Phone left, string right) => left.Equals(right);
    public static bool operator !=(Phone left, string right) => !left.Equals(right);
    public static bool operator ==(string left, Phone right) => left.Equals(right);
    public static bool operator !=(string left, Phone right) => !left.Equals(right);

    
    public static bool TryParse(string value, out Phone? phone)
    {
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


    [GeneratedRegex(@"^(\+?\d{1,3}\s?)?\(?\d{1,3}\)?[\s-]?\d{1,4}[\s-]?\d{1,2}[\s-]?\d{1,2}$")]
    private static partial Regex PhoneNumberRegex();
}


public class PhoneJsonConverter : JsonConverter<Phone>
{
    public override Phone Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var phoneString = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(phoneString) || !Phone.TryParse(phoneString, out var phone)) 
            throw new JsonException($"Invalid phone number format: '{phoneString}'");

        return phone!.Value;
    }
    
    public override void Write(Utf8JsonWriter writer, Phone phone, JsonSerializerOptions options) 
        => writer.WriteStringValue(phone);
}