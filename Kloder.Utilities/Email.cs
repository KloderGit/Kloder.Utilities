using System;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain;

[JsonConverter(typeof(EmailJsonConverter))]
public readonly struct Email : IEquatable<Email>
{
    private readonly string value = string.Empty;

    public Email(string value)
    {
        this.value = new MailAddress(value).Address;
    }

    public bool Equals(Email other)
    {
        return value.Equals(other.value, StringComparison.OrdinalIgnoreCase);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        var other = (Email)obj;
        return Equals(other);
    }
    
    public static bool operator ==(Email email1, Email email2)
    {
        return email1.value == email2.value;
    }
    
    public static bool operator !=(Email email1, Email email2)
    {
        return !(email1 == email2);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    
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
    
    public static implicit operator string(Email x)
    {
        return x.value;
    }
    
    public static explicit operator Email(string x)
    {
        try
        {
            var email = new Email(x);
            return email;
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.Message);
        }
    }    
    public override string ToString() => value;
}

public class EmailJsonConverter : JsonConverter<Email>
{
    public override Email Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var result = Email.TryParse(reader.GetString()!, out var email);
        
        return email.Value;
    }
    
    public override void Write(
        Utf8JsonWriter writer,
        Email email,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(email.ToString());
}