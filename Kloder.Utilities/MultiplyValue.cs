using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utilities;


public class  MultiplyValue<T> : IReadOnlyCollection<T>
{
    private readonly HashSet<T> _values = [];
    public T? Preferred { get; private set; }
    public int Count => _values.Count;

    // ReSharper disable once UnusedMember.Local
    private MultiplyValue() {}
    
    public MultiplyValue(IEnumerable<T> values, IEqualityComparer<T>? comparer = null)
    {
        _values = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

        foreach (var value in values)
            Add(value);
    }

    public MultiplyValue(T value, IEqualityComparer<T>? comparer = null)
    {
        _values = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);
        Add(value);
    }
    
    public void Add(T value)
    {
        var added = _values.Add(value);
        if (added && _values.Count == 1)
            SetDefault(value);
    }
    
    public void Remove(T value)
    {
        var isRemoved = _values.Remove(value);
        if (!isRemoved) return;
        
        if (EqualityComparer<T>.Default.Equals(Preferred, value) && _values.Count > 0)
            SetDefault(_values.First());
    }
    
    public void Clear()
    {
        _values.Clear();
    }

    public void SetDefault(T value)
    {
        if (_values.Contains(value) == false) Add(value);
        if (!Equals(Preferred, value)) Preferred = value;
    }

    public IEnumerator<T> GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}