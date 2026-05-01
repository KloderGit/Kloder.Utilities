using System;
using System.Collections.Generic;
using System.Text.Json;
using Utilities;

namespace Tests;

public class PatchTests
{
    private sealed record ItemPatchDto
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public int? Quantity { get; init; }
    }


    public class PatchStructTests
    {
        [Fact]
        public void Default_ShouldHaveNoValue()
        {
            Patch<string> patch = default;

            Assert.False(patch.HasValue);
            Assert.Null(patch.Value);
        }

        [Fact]
        public void Constructor_ShouldSetValueAndHasValue_WhenValueProvided()
        {
            const string expected = "test";

            var patch = new Patch<string>(expected);

            Assert.True(patch.HasValue);
            Assert.Equal(expected, patch.Value);
        }

        [Fact]
        public void Constructor_ShouldSetHasValueTrueAndNullValue_WhenNullProvided()
        {
            var patch = new Patch<string?>(null);

            Assert.True(patch.HasValue);
            Assert.Null(patch.Value);
        }

        [Fact]
        public void Constructor_ShouldWorkWithValueType()
        {
            var patch = new Patch<int>(42);

            Assert.True(patch.HasValue);
            Assert.Equal(42, patch.Value);
        }

        [Fact]
        public void ApplyAction_ShouldInvokeAction_WhenHasValue()
        {
            var patch = new Patch<string>("hello");
            string? captured = null;

            patch.Apply(v => captured = v);

            Assert.Equal("hello", captured);
        }

        [Fact]
        public void ApplyAction_ShouldNotInvokeAction_WhenDefault()
        {
            Patch<string> patch = default;
            var invoked = false;

            patch.Apply(_ => invoked = true);

            Assert.False(invoked);
        }

        [Fact]
        public void ApplyAction_ShouldPassNull_WhenValueIsNull()
        {
            var patch = new Patch<string?>(null);
            var invoked = false;
            string? captured = "not null";

            patch.Apply(v => { invoked = true; captured = v; });

            Assert.True(invoked);
            Assert.Null(captured);
        }

        [Fact]
        public void ApplyFunc_ShouldReturnMappedResult_WhenHasValue()
        {
            var patch = new Patch<int>(42);

            var result = patch.Apply(
                v => Result<int>.Success(v * 2),
                Result<int>.Success(0));

            Assert.True(result.IsSuccess);
            Assert.Equal(84, result.Value);
        }

        [Fact]
        public void ApplyFunc_ShouldReturnDefaultValue_WhenDefault()
        {
            Patch<int> patch = default;
            var fallback = Result<string>.Success("fallback");

            var result = patch.Apply(
                v => Result<string>.Success(v.ToString()!),
                fallback);

            Assert.True(result.IsSuccess);
            Assert.Equal("fallback", result.Value);
        }

        [Fact]
        public void ApplyFunc_ShouldPassNull_WhenValueIsNull()
        {
            var patch = new Patch<string?>(null);

            var result = patch.Apply(
                v => Result<string?>.Success(v ?? "was null"),
                Result<string?>.Success("default"));

            Assert.True(result.IsSuccess);
            Assert.Equal("was null", result.Value);
        }

        [Fact]
        public void Equality_ShouldReturnTrue_ForEqualPatches()
        {
            var first = new Patch<int>(10);
            var second = new Patch<int>(10);

            Assert.Equal(first, second);
            Assert.True(first == second);
        }

        [Fact]
        public void Equality_ShouldReturnFalse_ForDifferentValues()
        {
            var first = new Patch<int>(10);
            var second = new Patch<int>(20);

            Assert.NotEqual(first, second);
            Assert.True(first != second);
        }

        [Fact]
        public void Equality_ShouldReturnFalse_BetweenDefaultAndPatchedNull()
        {
            Patch<string?> defaultPatch = default;
            var patchedNull = new Patch<string?>(null);

            Assert.NotEqual(defaultPatch, patchedNull);
        }
    }


    public class PatchJsonTests
    {
        [Fact]
        public void Serialize_ShouldWriteValue_WhenHasValue()
        {
            var patch = new Patch<string>("hello");

            var json = JsonSerializer.Serialize(patch);

            Assert.Equal("\"hello\"", json);
        }

        [Fact]
        public void Serialize_ShouldWriteNull_WhenValueIsNull()
        {
            var patch = new Patch<string?>(null);

            var json = JsonSerializer.Serialize(patch);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Serialize_ShouldWriteNull_WhenHasValueFalse()
        {
            Patch<string> patch = default;

            var json = JsonSerializer.Serialize(patch);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Deserialize_ShouldReturnPatchWithValue_WhenJsonHasValue()
        {
            const string json = "\"hello\"";

            var patch = JsonSerializer.Deserialize<Patch<string>>(json);

            Assert.True(patch.HasValue);
            Assert.Equal("hello", patch.Value);
        }

        [Fact]
        public void Deserialize_ShouldReturnPatchWithDefaultValue_WhenJsonIsNull()
        {
            const string json = "null";

            var patch = JsonSerializer.Deserialize<Patch<string>>(json);

            Assert.True(patch.HasValue);
            Assert.Null(patch.Value);
        }

        [Fact]
        public void Deserialize_ShouldKeepHasValueFalse_WhenPropertyIsAbsent()
        {
            const string json = "{}";

            var dto = JsonSerializer.Deserialize<PatchableDto>(json);

            Assert.NotNull(dto);
            Assert.False(dto!.Name.HasValue);
        }

        [Fact]
        public void Deserialize_ShouldSetHasValueTrueWithNull_WhenPropertyIsExplicitNull()
        {
            const string json = "{\"Name\":null}";

            var dto = JsonSerializer.Deserialize<PatchableDto>(json);

            Assert.NotNull(dto);
            Assert.True(dto!.Name.HasValue);
            Assert.Null(dto.Name.Value);
        }

        [Fact]
        public void Deserialize_ShouldSetHasValueTrueWithValue_WhenPropertyIsPresent()
        {
            const string json = "{\"Name\":\"hello\"}";

            var dto = JsonSerializer.Deserialize<PatchableDto>(json);

            Assert.NotNull(dto);
            Assert.True(dto!.Name.HasValue);
            Assert.Equal("hello", dto.Name.Value);
        }

        [Fact]
        public void Roundtrip_ShouldPreserveValue_ForComplexType()
        {
            var original = new Patch<ItemPatchDto>(new ItemPatchDto
            {
                Id = Guid.NewGuid(),
                Name = "Item",
                Quantity = 5
            });

            var json = JsonSerializer.Serialize(original);
            var restored = JsonSerializer.Deserialize<Patch<ItemPatchDto>>(json);

            Assert.True(restored.HasValue);
            Assert.NotNull(restored.Value);
            Assert.Equal(original.Value!.Id, restored.Value!.Id);
            Assert.Equal(original.Value.Name, restored.Value.Name);
            Assert.Equal(original.Value.Quantity, restored.Value.Quantity);
        }


        private sealed class PatchableDto
        {
            public Patch<string?> Name { get; set; }
        }
    }


    public class CollectionPatchTests
    {
        [Fact]
        public void Default_ShouldBeEmpty()
        {
            CollectionPatch<ItemPatchDto> patch = default;

            Assert.False(patch.HasAdded);
            Assert.False(patch.HasRemoved);
            Assert.False(patch.HasUpdated);
            Assert.True(patch.IsEmpty);
            Assert.Null(patch.Added);
            Assert.Null(patch.RemovedIds);
            Assert.Null(patch.Updated);
        }

        [Fact]
        public void EmptyCollections_ShouldBeTreatedAsEmpty()
        {
            var patch = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto>(),
                RemovedIds = new List<Guid>(),
                Updated = new List<ItemPatchDto>()
            };

            Assert.False(patch.HasAdded);
            Assert.False(patch.HasRemoved);
            Assert.False(patch.HasUpdated);
            Assert.True(patch.IsEmpty);
        }

        [Fact]
        public void HasAdded_ShouldBeTrue_WhenAddedHasItems()
        {
            var patch = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = Guid.NewGuid(), Name = "A" } }
            };

            Assert.True(patch.HasAdded);
            Assert.False(patch.HasRemoved);
            Assert.False(patch.HasUpdated);
            Assert.False(patch.IsEmpty);
        }

        [Fact]
        public void HasRemoved_ShouldBeTrue_WhenRemovedIdsHasItems()
        {
            var patch = new CollectionPatch<ItemPatchDto>
            {
                RemovedIds = new List<Guid> { Guid.NewGuid() }
            };

            Assert.False(patch.HasAdded);
            Assert.True(patch.HasRemoved);
            Assert.False(patch.HasUpdated);
            Assert.False(patch.IsEmpty);
        }

        [Fact]
        public void HasUpdated_ShouldBeTrue_WhenUpdatedHasItems()
        {
            var patch = new CollectionPatch<ItemPatchDto>
            {
                Updated = new List<ItemPatchDto> { new() { Id = Guid.NewGuid(), Name = "U" } }
            };

            Assert.False(patch.HasAdded);
            Assert.False(patch.HasRemoved);
            Assert.True(patch.HasUpdated);
            Assert.False(patch.IsEmpty);
        }

        [Fact]
        public void AllFlags_ShouldBeTrue_WhenAllCollectionsPopulated()
        {
            var patch = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = Guid.NewGuid(), Name = "A" } },
                RemovedIds = new List<Guid> { Guid.NewGuid() },
                Updated = new List<ItemPatchDto> { new() { Id = Guid.NewGuid(), Name = "U" } }
            };

            Assert.True(patch.HasAdded);
            Assert.True(patch.HasRemoved);
            Assert.True(patch.HasUpdated);
            Assert.False(patch.IsEmpty);
        }

        [Fact]
        public void Roundtrip_ShouldPreserveAllCollections()
        {
            var addedId = Guid.NewGuid();
            var removedId = Guid.NewGuid();
            var updatedId = Guid.NewGuid();

            var original = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = addedId, Name = "Added", Quantity = 1 } },
                RemovedIds = new List<Guid> { removedId },
                Updated = new List<ItemPatchDto> { new() { Id = updatedId, Name = "Updated", Quantity = 2 } }
            };

            var json = JsonSerializer.Serialize(original);
            var restored = JsonSerializer.Deserialize<CollectionPatch<ItemPatchDto>>(json);

            Assert.True(restored.HasAdded);
            Assert.True(restored.HasRemoved);
            Assert.True(restored.HasUpdated);

            Assert.Single(restored.Added!);
            Assert.Equal(addedId, restored.Added![0].Id);

            Assert.Single(restored.RemovedIds!);
            Assert.Equal(removedId, restored.RemovedIds![0]);

            Assert.Single(restored.Updated!);
            Assert.Equal(updatedId, restored.Updated![0].Id);
        }
    }


    public class PatchOfCollectionPatchTests
    {
        [Fact]
        public void Default_ShouldHaveNoValue()
        {
            Patch<CollectionPatch<ItemPatchDto>> patch = default;

            Assert.False(patch.HasValue);
        }

        [Fact]
        public void Constructor_ShouldWrapCollectionPatch()
        {
            var inner = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = Guid.NewGuid(), Name = "A" } }
            };

            var patch = new Patch<CollectionPatch<ItemPatchDto>>(inner);

            Assert.True(patch.HasValue);
            Assert.True(patch.Value.HasAdded);
            Assert.False(patch.Value.HasRemoved);
            Assert.False(patch.Value.HasUpdated);
        }

        [Fact]
        public void Serialize_ShouldEmitNull_WhenHasValueFalse()
        {
            Patch<CollectionPatch<ItemPatchDto>> patch = default;

            var json = JsonSerializer.Serialize(patch);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Serialize_ShouldEmitInnerCollectionPatch_WhenHasValue()
        {
            var addedId = Guid.NewGuid();
            var inner = new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = addedId, Name = "A", Quantity = 1 } }
            };
            var patch = new Patch<CollectionPatch<ItemPatchDto>>(inner);

            var json = JsonSerializer.Serialize(patch);

            Assert.Contains("\"Added\"", json);
            Assert.Contains(addedId.ToString(), json);
        }

        [Fact]
        public void Deserialize_ShouldReturnPatchWithDefaultCollection_WhenJsonIsNull()
        {
            const string json = "null";

            var patch = JsonSerializer.Deserialize<Patch<CollectionPatch<ItemPatchDto>>>(json);

            Assert.True(patch.HasValue);
            Assert.True(patch.Value.IsEmpty);
        }

        [Fact]
        public void Roundtrip_ShouldPreserveAllOperations()
        {
            var addedId = Guid.NewGuid();
            var removedId = Guid.NewGuid();
            var updatedId = Guid.NewGuid();

            var original = new Patch<CollectionPatch<ItemPatchDto>>(new CollectionPatch<ItemPatchDto>
            {
                Added = new List<ItemPatchDto> { new() { Id = addedId, Name = "Added", Quantity = 1 } },
                RemovedIds = new List<Guid> { removedId },
                Updated = new List<ItemPatchDto> { new() { Id = updatedId, Name = "Updated", Quantity = 2 } }
            });

            var json = JsonSerializer.Serialize(original);
            var restored = JsonSerializer.Deserialize<Patch<CollectionPatch<ItemPatchDto>>>(json);

            Assert.True(restored.HasValue);

            var collection = restored.Value;
            Assert.True(collection.HasAdded);
            Assert.True(collection.HasRemoved);
            Assert.True(collection.HasUpdated);

            Assert.Equal(addedId, collection.Added![0].Id);
            Assert.Equal("Added", collection.Added[0].Name);
            Assert.Equal(1, collection.Added[0].Quantity);

            Assert.Equal(removedId, collection.RemovedIds![0]);

            Assert.Equal(updatedId, collection.Updated![0].Id);
            Assert.Equal("Updated", collection.Updated[0].Name);
            Assert.Equal(2, collection.Updated[0].Quantity);
        }

        [Fact]
        public void Deserialize_ShouldKeepOuterPatchHasValueFalse_WhenPropertyAbsent()
        {
            const string json = "{}";

            var dto = JsonSerializer.Deserialize<EnvelopeDto>(json);

            Assert.NotNull(dto);
            Assert.False(dto!.Items.HasValue);
        }

        [Fact]
        public void Deserialize_ShouldSetOuterPatchToEmptyCollection_WhenPropertyIsExplicitNull()
        {
            const string json = "{\"Items\":null}";

            var dto = JsonSerializer.Deserialize<EnvelopeDto>(json);

            Assert.NotNull(dto);
            Assert.True(dto!.Items.HasValue);
            Assert.True(dto.Items.Value.IsEmpty);
        }

        [Fact]
        public void Deserialize_ShouldPopulateInnerCollection_WhenPropertyHasValue()
        {
            var addedId = Guid.NewGuid();
            var json = "{\"Items\":{\"Added\":[{\"Id\":\"" + addedId + "\",\"Name\":\"A\",\"Quantity\":3}]}}";

            var dto = JsonSerializer.Deserialize<EnvelopeDto>(json);

            Assert.NotNull(dto);
            Assert.True(dto!.Items.HasValue);

            var collection = dto.Items.Value;
            Assert.True(collection.HasAdded);
            Assert.False(collection.HasRemoved);
            Assert.False(collection.HasUpdated);
            Assert.Single(collection.Added!);
            Assert.Equal(addedId, collection.Added![0].Id);
            Assert.Equal("A", collection.Added[0].Name);
            Assert.Equal(3, collection.Added[0].Quantity);
        }


        private sealed class EnvelopeDto
        {
            public Patch<CollectionPatch<ItemPatchDto>> Items { get; set; }
        }
    }
}
