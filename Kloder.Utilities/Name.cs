using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;

[JsonConverter(typeof(NameJsonConverter))]
public record Name
{
    public string LastName { get; }
    public string FirstName { get; }
    public string Patronymic { get; }

    public Name(string lastName, string firstName, string patronymic = "")
    {
        if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName)) 
            throw new ArgumentException("Фамилия или Имя не указаны");
        
        LastName = lastName.Trim();
        FirstName = firstName.Trim();
        Patronymic = patronymic?.Trim() ?? "";
    }
    
    public Name(string fullName)
        : this(Parse(fullName)) {}

    private Name(string[] values)
        : this(values[0], values[1], values.Length > 2 ? values[2] : "") {}


    public string Initials() => $"{LastName} {GetLetterOfName(FirstName)}{GetLetterOfName(Patronymic)}";
    public string ShortName() => $"{LastName} {FirstName}";
    public string FullName() => string.IsNullOrWhiteSpace(Patronymic) ? $"{LastName} {FirstName}" : $"{LastName} {FirstName} {Patronymic}";
    public override string ToString() => FullName();
    
    private static string[] Parse(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ФИО не указано");
        
        var values = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length < 2)
            throw new ArgumentException("ФИО должно содержать минимум фамилию и имя");
        return values;
    }
    private string GetLetterOfName(string part) => string.IsNullOrWhiteSpace(part) == false ? $"{part[0]}." : "";
}


public class NameJsonConverter : JsonConverter<Name?>
{
    public override Name? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var s = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(s))
            return null;
        
        return new Name(s);
    }

    public override void Write(Utf8JsonWriter writer, Name? name, JsonSerializerOptions options)
    {
        if (name is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(name.ToString());
    }
}