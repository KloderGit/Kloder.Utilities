using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Utilities;

[DebuggerDisplay("{_value}")]
[JsonConverter(typeof(EmailJsonConverter))]
public readonly partial struct Email
{
    private readonly string _value = string.Empty;
    
    public Email(string value)
    {
        var emailString = value.Trim().ToLower();
        if (IsValidEmail(emailString) == false) throw new ArgumentException("Invalid email address", nameof(value));
        _value = emailString;
    }

    public override string ToString() => _value;
    
    public bool Equals(Email other) => _value == other._value;
    public bool Equals(string other) => _value.Equals(other.Trim(), StringComparison.CurrentCultureIgnoreCase);
    public override bool Equals(object? obj)
    {
        var result = obj switch
        {
            Email other => Equals(other),
            string other => Equals(other),
            _ => false
        };
        
        return result;
    }

    public override int GetHashCode() => _value.GetHashCode();
    
    
    public static implicit operator string(Email x) => x._value;
    public static explicit operator Email(string x) => new(x);
    
    
    public static bool operator ==(Email left, Email right) => left.Equals(right);
    public static bool operator !=(Email left, Email right) => !left.Equals(right);
    
    public static bool operator ==(Email left, string right) => left.Equals(right);
    public static bool operator !=(Email left, string right) => !left.Equals(right);
    
    public static bool operator ==(string left, Email right) => left.Equals(right);
    public static bool operator !=(string left, Email right) => !left.Equals(right);
    
    
    private static bool IsValidEmail(string email) => EmailTemplateRegExp().IsMatch(email);
    
    
    public static bool TryParse(string value, out Email? email)
    {
        try
        {
            email = new Email(value);
            return true;
        }
        catch
        {
            email = null;
            return false;
        }
    }
    
    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex EmailTemplateRegExp();
}


public class EmailJsonConverter : JsonConverter<Email>
{
    public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var emailString = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(emailString) || !Email.TryParse(emailString, out var email)) 
            throw new JsonException($"Invalid email address format: '{emailString}'");

        return email!.Value;
    }
    
    public override void Write(Utf8JsonWriter writer, Email email, JsonSerializerOptions options) 
        => writer.WriteStringValue(email.ToString());
}

public class NullableEmailJsonConverter : JsonConverter<Email?>
{
    public override Email? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var emailString = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(emailString) || !Email.TryParse(emailString, out var email)) 
            return null;

        return email!.Value;
    }
    
    public override void Write(Utf8JsonWriter writer, Email? email, JsonSerializerOptions options)
    {
        if(email is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(email.ToString());
        }
    }
}