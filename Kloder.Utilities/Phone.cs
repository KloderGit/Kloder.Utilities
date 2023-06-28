using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Domain;

[JsonConverter(typeof(PhoneJsonConverter))]
public readonly struct Phone : IEquatable<Phone?>
{
    private const string DigitsOnlyPattern = "[^0-9]";
    private const string PhoneNumberPattern = @"^(\+?\d{1,3}\s?)?\(?\d{1,3}\)?[\s-]?\d{1,4}[\s-]?\d{1,2}[\s-]?\d{1,2}$";
    private string OnlyDigitsValue => string.IsNullOrEmpty(value) 
        ? string.Empty 
        : Regex.Replace(value, DigitsOnlyPattern, String.Empty);

    private readonly string value = string.Empty;

    public Phone(string value)
    {
        var phoneString = value.Trim();

        if (string.IsNullOrWhiteSpace(phoneString)) throw new ArgumentException(nameof(value));

        if (Regex.IsMatch(phoneString, PhoneNumberPattern) == false)
            throw new FormatException("Неверный формат телефона.");

        if (phoneString[0] == '8') phoneString ="+7" + phoneString.Substring(1);
        
        this.value = phoneString;
    }

    public bool Equals(Phone? other)
    {
        if (other == null) return false; 
        return OnlyDigitsValue == other?.OnlyDigitsValue;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if ((obj is Phone) == false) return false;
        var other = (Phone)obj;
        return Equals(other);
    }
    
    public static bool operator ==(Phone phone1, Phone phone2)
    {
        return phone1.Equals(phone2);
    }
    
    public static bool operator !=(Phone phone1, Phone phone2)
    {
        return !(phone1 == phone2);
    }

    public override int GetHashCode()
    {
        return OnlyDigitsValue.GetHashCode();
    }

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

    public static implicit operator string(Phone x)
    {
        return x.value;
    }
    
    public static explicit operator Phone(string x)
    {
        try
        {
            var phone = new Phone(x);
            return phone;
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.Message);
        }
    }
    public override string ToString() => value;
}


public class PhoneJsonConverter : JsonConverter<Phone>
{
    public override Phone Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var result = Phone.TryParse(reader.GetString()!, out var phone);
        
        return phone.Value;
    }
    
    public override void Write(
        Utf8JsonWriter writer,
        Phone phone,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(phone.ToString());
}