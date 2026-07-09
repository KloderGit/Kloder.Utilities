using Utilities;

namespace Tests;

public class MultiplyValueTests
{
    [Fact]
    public void Enumeration_ShouldPreserveInsertionOrder()
    {
        var collection = new MultiplyValue<string>(["c", "a", "b"]);

        Assert.Equal(["c", "a", "b"], collection.ToList());
    }

    [Fact]
    public void Add_ShouldNotDuplicate_AndKeepFirstPosition()
    {
        var collection = new MultiplyValue<string>(["a", "b"]);
        collection.Add("a");

        Assert.Equal(2, collection.Count);
        Assert.Equal(["a", "b"], collection.ToList());
    }

    [Fact]
    public void Preferred_ShouldBeFirstAddedValue()
    {
        var collection = new MultiplyValue<string>(["b", "a"]);

        Assert.Equal("b", collection.Preferred);
    }

    [Fact]
    public void Remove_ShouldMovePreferredToOldestRemaining_WhenPreferredRemoved()
    {
        var collection = new MultiplyValue<string>(["a", "b", "c"]);
        collection.Remove("a");

        Assert.Equal("b", collection.Preferred);
        Assert.Equal(["b", "c"], collection.ToList());
    }

    [Fact]
    public void Remove_ShouldKeepPreferred_WhenOtherValueRemoved()
    {
        var collection = new MultiplyValue<string>(["a", "b", "c"]);
        collection.SetDefault("b");
        collection.Remove("c");

        Assert.Equal("b", collection.Preferred);
    }

    [Fact]
    public void Remove_ShouldResetPreferred_WhenLastValueRemoved()
    {
        var collection = new MultiplyValue<string>("a");
        collection.Remove("a");

        Assert.Empty(collection);
        Assert.Null(collection.Preferred);
    }

    [Fact]
    public void Clear_ShouldResetPreferred()
    {
        var collection = new MultiplyValue<string>(["a", "b"]);
        collection.Clear();

        Assert.Empty(collection);
        Assert.Null(collection.Preferred);
    }

    [Fact]
    public void Add_ShouldRestorePreferred_WhenCollectionWasEmptied()
    {
        var collection = new MultiplyValue<string>("a");
        collection.Remove("a");
        collection.Add("b");

        Assert.Equal("b", collection.Preferred);
    }

    [Fact]
    public void Remove_ShouldUseCustomComparer_ForPreferredCheck()
    {
        var collection = new MultiplyValue<string>(["A", "b"], StringComparer.OrdinalIgnoreCase);
        collection.Remove("a");

        Assert.Equal("b", collection.Preferred);
        Assert.Single(collection);
    }

    [Fact]
    public void Contains_ShouldRespectComparer()
    {
        var collection = new MultiplyValue<string>("A", StringComparer.OrdinalIgnoreCase);

        Assert.True(collection.Contains("a"));
        Assert.False(collection.Contains("b"));
    }
}
