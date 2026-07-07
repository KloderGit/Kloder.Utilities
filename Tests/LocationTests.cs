using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Utilities;

namespace Tests;

public class LocationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public class BuildingLocationSerializationTests
    {
        [Fact]
        public void Serialize_ShouldContainPlacementType()
        {
            Location location = new BuildingLocation(
                new Title("Офис"), "Москва", "Ленина", "10", null);

            var json = JsonSerializer.Serialize(location, Options);

            Assert.Contains("\"PlacementType\"", json);
            Assert.Contains("0", json); // PlacementType.Building == 0
        }

        [Fact]
        public void Serialize_BuildingLocation_ShouldContainAllFields()
        {
            Location location = new BuildingLocation(
                new Title("Офис"), "Москва", "Ленина", "10", "2", 101000, "Описание");

            var json = JsonSerializer.Serialize(location, Options);

            Assert.Contains("Москва", json);
            Assert.Contains("Ленина", json);
            Assert.Contains("10", json);
            Assert.Contains("2", json);
            Assert.Contains("101000", json);
            Assert.Contains("Описание", json);
        }

        [Fact]
        public void Deserialize_BuildingLocation_ShouldRestoreAllFields()
        {
            Location original = new BuildingLocation(
                new Title("Склад"), "Санкт-Петербург", "Невский", "5", "3", 190000, "Основной склад");

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            var building = Assert.IsType<BuildingLocation>(restored);
            Assert.Equal("Санкт-Петербург", building.City);
            Assert.Equal("Невский", building.Street);
            Assert.Equal("5", building.BuildingNumber);
            Assert.Equal("3", building.EntranceNumber);
            Assert.Equal(190000u, building.Zip);
            Assert.Equal("Основной склад", building.Description);
            Assert.Equal(PlacementType.Building, building.PlacementType);
        }

        [Fact]
        public void Deserialize_BuildingLocation_WithoutEntrance_ShouldHaveNullEntrance()
        {
            Location original = new BuildingLocation(
                new Title("Офис"), "Казань", "Пушкина", "7", null);

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            var building = Assert.IsType<BuildingLocation>(restored);
            Assert.Null(building.EntranceNumber);
        }

        [Fact]
        public void RoundTrip_BuildingLocation_ShouldPreserveTitle()
        {
            Location original = new BuildingLocation(
                new Title("Главный офис"), "Москва", "Арбат", "1", null);

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.Equal((string)original.Title, (string)restored!.Title);
        }
    }

    public class InternetLocationSerializationTests
    {
        [Fact]
        public void Serialize_InternetLocation_ShouldContainUri()
        {
            Location location = new InternetLocation(
                new Title("Сайт"), new Uri("https://example.com"), "Описание");

            var json = JsonSerializer.Serialize(location, Options);

            Assert.Contains("example.com", json);
            Assert.Contains("Описание", json);
        }

        [Fact]
        public void Deserialize_InternetLocation_ShouldRestoreUri()
        {
            Location original = new InternetLocation(
                new Title("Портал"), new Uri("https://portal.example.org/path?q=1"));

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            var internet = Assert.IsType<InternetLocation>(restored);
            Assert.Equal("portal.example.org", internet.Uri.Host);
            Assert.Equal(PlacementType.Internet, internet.PlacementType);
        }

        [Fact]
        public void RoundTrip_InternetLocation_ShouldPreservePlacementType()
        {
            Location original = new InternetLocation(
                new Title("API"), new Uri("https://api.example.com"));

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.Equal(PlacementType.Internet, restored!.PlacementType);
        }

        [Fact]
        public void RoundTrip_InternetLocation_ShouldPreserveTitle()
        {
            Location original = new InternetLocation(
                new Title("Документация"), new Uri("https://docs.example.com"));

            var json = JsonSerializer.Serialize(original, Options);
            var restored = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.Equal((string)original.Title, (string)restored!.Title);
        }
    }

    public class PolymorphicDeserializationTests
    {
        [Fact]
        public void Deserialize_BuildingJson_ShouldReturnBuildingLocation()
        {
            var json = """
                {
                    "PlacementType": 0,
                    "Title": "Офис",
                    "City": "Москва",
                    "Street": "Тверская",
                    "BuildingNumber": "1",
                    "EntranceNumber": null,
                    "Zip": 0,
                    "Description": ""
                }
                """;

            var location = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.IsType<BuildingLocation>(location);
        }

        [Fact]
        public void Deserialize_InternetJson_ShouldReturnInternetLocation()
        {
            var json = """
                {
                    "PlacementType": 1,
                    "Title": "Сайт",
                    "uri": "https://example.com",
                    "Description": ""
                }
                """;

            var location = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.IsType<InternetLocation>(location);
        }

        [Fact]
        public void Deserialize_WithStringPlacementType_ShouldWork()
        {
            var json = """
                {
                    "PlacementType": "Building",
                    "Title": "Склад",
                    "City": "Новосибирск",
                    "Street": "Центральная",
                    "BuildingNumber": "99",
                    "EntranceNumber": null,
                    "Zip": 0,
                    "Description": ""
                }
                """;

            var location = JsonSerializer.Deserialize<Location>(json, Options);

            Assert.IsType<BuildingLocation>(location);
        }

        [Fact]
        public void Deserialize_MissingPlacementType_ShouldThrowJsonException()
        {
            var json = """{ "Title": "Офис", "City": "Москва" }""";

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<Location>(json, Options));
        }

        [Fact]
        public void Deserialize_UnsupportedPlacementType_ShouldThrowNotSupportedException()
        {
            var json = """{ "PlacementType": 2, "Title": "Улица" }""";

            Assert.Throws<NotSupportedException>(() =>
                JsonSerializer.Deserialize<Location>(json, Options));
        }
    }
}
