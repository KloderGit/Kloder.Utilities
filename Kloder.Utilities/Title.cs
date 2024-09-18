using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;

[DebuggerDisplay("{_value}")]
public record Title
{
    private string _value;

    public Title()
    {
        _value = string.Empty;
    }
    public Title(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Значение Title не указано");
        _value = value;
    }
    
    public static implicit operator string(Title x) => x._value;
    public static explicit operator Title(string x) => new Title(x);
}

public class TitleJsonConverter : JsonConverter<Title>
{
    public override Title Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var titleString = reader.GetString();
        
        var title = string.IsNullOrEmpty(titleString)
            ? new Title()
            : new Title(titleString);

        return title;
    }
    
    public override void Write(Utf8JsonWriter writer, Title title, JsonSerializerOptions options) 
        => writer.WriteStringValue(title);
}