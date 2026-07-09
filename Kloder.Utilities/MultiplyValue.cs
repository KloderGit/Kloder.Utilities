using System.Collections;
using System.Collections.Generic;

namespace Utilities;


public class  MultiplyValue<T> : IReadOnlyCollection<T> where T : notnull
{
    private readonly Dictionary<T, LinkedListNode<T>> _nodes = new();
    private readonly LinkedList<T> _order = new();
    public T? Preferred { get; private set; }
    public int Count => _nodes.Count;

    public MultiplyValue() {}

    public MultiplyValue(IEnumerable<T> values, IEqualityComparer<T>? comparer = null)
    {
        _nodes = new Dictionary<T, LinkedListNode<T>>(comparer ?? EqualityComparer<T>.Default);

        foreach (var value in values)
            Add(value);
    }

    public MultiplyValue(T value, IEqualityComparer<T>? comparer = null)
    {
        _nodes = new Dictionary<T, LinkedListNode<T>>(comparer ?? EqualityComparer<T>.Default);
        Add(value);
    }

    public void Add(T value)
    {
        if (_nodes.ContainsKey(value)) return;
        _nodes[value] = _order.AddLast(value);
        if (_nodes.Count == 1)
            SetDefault(value);
    }

    public void Remove(T value)
    {
        if (!_nodes.Remove(value, out var node)) return;
        _order.Remove(node);

        if (_nodes.Comparer.Equals(Preferred, value))
            Preferred = _order.First is { } first ? first.Value : default;
    }

    public void Clear()
    {
        _nodes.Clear();
        _order.Clear();
        Preferred = default;
    }

    public void SetDefault(T value)
    {
        if (_nodes.ContainsKey(value) == false) Add(value);
        Preferred = value;
    }

    public bool Contains(T value) => _nodes.ContainsKey(value);

    public IEnumerator<T> GetEnumerator() => _order.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
