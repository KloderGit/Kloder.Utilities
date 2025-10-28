using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utilities;


public abstract record Location
{
    public Title Title { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public PlacementType PlacementType { get; protected set; }
    public abstract string GetShortAddress();
    public abstract string GetAddress();
    public abstract bool IsSameLocation(Location other);
}

public abstract record Location<T> : Location
{
    protected internal Key<T> Key { get; set; }
}

public record InternetLocation : Location<InternetLocation>
{
    public Uri uri { get; }

    internal InternetLocation(Title title, string url, string? description = null)
    {
        Key = new Key<InternetLocation>();
        Title = title;
        Description = description ?? "";
        PlacementType = PlacementType.Internet;
        uri = new Uri(url);
    }

    public override string GetShortAddress() => uri.Host;
    public override string GetAddress() => uri.AbsolutePath;

    public override bool IsSameLocation(Location other)
    {
        if (other is InternetLocation == false) return false;
        return GetShortAddress() == other.GetShortAddress();
    }
}

public record BuildingLocation : Location<BuildingLocation>
{
    public uint Zip { get; }
    public string City { get; }
    public string Street { get; }
    public string BuildingNumber { get; }
    public string? EntranceNumber { get; }

    public BuildingLocation(
        Title title,
        string city,
        string street,
        string buildingNumber,
        string? entranceNumber,
        uint zip = 0,
        string? description = null)
    {
        Key = new Key<BuildingLocation>();
        Title = title;
        PlacementType = PlacementType.Building;
        Zip = zip;
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
        EntranceNumber = entranceNumber;
        Description = description ?? "";
    }

    public override string GetShortAddress() => $"{City}, {Street}, д/стр.: {BuildingNumber}";

    public override string GetAddress() =>
        $"{Zip}, {GetShortAddress()}{(string.IsNullOrEmpty(EntranceNumber) ? string.Empty : $", под.: {EntranceNumber}")}";

    public override bool IsSameLocation(Location other)
    {
        if (other is BuildingLocation item == false) return false;
        return City == item.City
               && Street == item.Street
               && BuildingNumber == item.BuildingNumber;
    }
}
//
// public record OutdoorLocation : Location<OutdoorLocation>
// {
//     private readonly string city;
//     private readonly string street;
//     private readonly string nearBuildingNumber;
//     private readonly string comment;
//
//     private readonly string longitude;
//     private readonly string altitude;
//     
//     public OutdoorLocation(string city, string street, string longitude = "", string altitude ="")
//     {
//         this.city = city;
//         this.street = street;
//         this.longitude = longitude;
//         this.altitude = altitude;
//         this.PlacementType = PlacementType.Outdoor;
//     }
//     public override string GetShortAddress() =>  $"{city}, {street}";
//
//     public override string GetAddress() => GetShortAddress();
//     
//     public override bool IsSameLocation(Location other)
//     {
//         this == other;
//     }
// }

public enum PlacementType
{
    Building,
    Internet,
    Outdoor
}


public sealed class LocationJsonConverter : JsonConverter<Location>
{
    public override Location? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(nameof(Location.PlacementType), out var ptProp))
            throw new JsonException($"'{nameof(Location.PlacementType)}' is required for Location polymorphic deserialization.");

        var placementType = ptProp.ValueKind switch
        {
            JsonValueKind.Number => (PlacementType)ptProp.GetInt32(),
            JsonValueKind.String when Enum.TryParse<PlacementType>(ptProp.GetString(), true, out var e) => e,
            _ => throw new JsonException($"Invalid '{nameof(Location.PlacementType)}' token kind: {ptProp.ValueKind}.")
        };

        var targetType = placementType switch
        {
            PlacementType.Building => typeof(BuildingLocation),
            PlacementType.Internet => typeof(InternetLocation),
            //PlacementType.Outdoor  => typeof(OutdoorLocation),  // TODO: реализовать этот тип
            _ => throw new NotSupportedException($"Unsupported PlacementType '{placementType}'.")
        };

        var json = root.GetRawText();
        return (Location?)JsonSerializer.Deserialize(json, targetType, options);
    }

    public override void Write(Utf8JsonWriter writer, Location value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case BuildingLocation b:
                JsonSerializer.Serialize(writer, b, options);
                return;
            case InternetLocation i: // TODO: реализовать этот тип
                JsonSerializer.Serialize(writer, i, options);
                return;
            // case OutdoorLocation o:  // TODO: реализовать этот тип
            //     JsonSerializer.Serialize(writer, o, options);
            //     return;
            default:
                throw new NotSupportedException($"Unsupported Location subtype '{value.GetType()}'.");
        }
    }
}