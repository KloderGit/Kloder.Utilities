using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Utilities;

public record Period : IEnumerable<DateOnly>, IComparable<Period>
{
    private readonly DateOnly _start;
    private readonly DateOnly _finish;

    public DateOnly Start => _start;
    public DateOnly Finish => _finish;
    public int Duration => _finish.DayNumber - _start.DayNumber + 1;

    private Period() {}

    public Period(DateOnly start, DateOnly finish)
    {
        var (validatedStart, validatedFinish) = ValidateParams(start, finish);

        _start = validatedStart;
        _finish = validatedFinish;
    }
    
    public bool IsIntersect(Period other) => !ReferenceEquals(other, null) && IsIntersect(this, other);

    public bool IsIntersect(DateOnly start, DateOnly finish)
    {
        var (validatedStart, validatedFinish) = ValidateParams(start, finish);
        
        var other = new Period(validatedStart, validatedFinish);

        return IsIntersect(other);
    }

    public IReadOnlyCollection<DateOnly> Intersection(Period other)
    {
        if (ReferenceEquals(other, null)) return ReadOnlyCollection<DateOnly>.Empty;

        var start = Start > other.Start ? Start : other.Start;
        var end = Finish < other.Finish ? Finish : other.Finish;

        if (start > end) return Array.Empty<DateOnly>();

        var intersectionDates = new List<DateOnly>();
        for (DateOnly date = start; date <= end; date = date.AddDays(1))
        {
            intersectionDates.Add(date);
        }

        return intersectionDates.AsReadOnly();
    }

    public int IntersectCount(Period other) => ReferenceEquals(other, null) ? 0 : IntersectCount(this, other);

    public bool ContainsDate(DateOnly date) => date.DayNumber >= Start.DayNumber && date.DayNumber <= Finish.DayNumber;

    public bool ContainsPeriod(Period other) => !ReferenceEquals(other, null) && (Start <= other.Start && other.Finish <= Finish);

    public IEnumerator<DateOnly> GetEnumerator()
    {
        for (var date = Start; date <= Finish; date = date.AddDays(1)) yield return date;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int CompareTo(Period? other) => other == null ? 1 : Start.CompareTo(other.Start);

    public int CompareTo(Period? other, PeriodComparerTypeEnum type)
    {
        if (other == null) return 1;

        return type switch
        {
            PeriodComparerTypeEnum.Start => Start.CompareTo(other.Start),
            PeriodComparerTypeEnum.Finish => Finish.CompareTo(other.Finish),
            PeriodComparerTypeEnum.Duration => Duration.CompareTo(other.Duration),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    private (DateOnly start, DateOnly finish) ValidateParams(DateOnly start, DateOnly finish)
    {
        if (start > finish) (start, finish) = (finish, start);
        return (start, finish);
    }
    
    public static bool operator <(Period p1, Period p2) => p1.CompareTo(p2) < 0;
    public static bool operator >(Period p1, Period p2) => p1.CompareTo(p2) > 0;
    public static bool operator <=(Period p1, Period p2) => p1.CompareTo(p2) <= 0;
    public static bool operator >=(Period p1, Period p2) => p1.CompareTo(p2) >= 0;

    public static bool operator <(Period p1, (Period, PeriodComparerTypeEnum) p2) => p1.CompareTo(p2.Item1, p2.Item2) < 0;
    public static bool operator >(Period p1, (Period, PeriodComparerTypeEnum) p2) => p1.CompareTo(p2.Item1, p2.Item2) > 0;
    public static bool operator <=(Period p1, (Period, PeriodComparerTypeEnum) p2) => p1.CompareTo(p2.Item1, p2.Item2) <= 0;
    public static bool operator >=(Period p1, (Period, PeriodComparerTypeEnum) p2) => p1.CompareTo(p2.Item1, p2.Item2) >= 0;


    private static bool IsIntersect(Period a, Period b) => a.Start <= b.Finish && a.Finish >= b.Start;
    
    
    public static int IntersectCount(Period a, Period b)
    {
        if (a.IsIntersect(b) == false) return 0;
        
        var (shortest, longest) = a.Duration < b.Duration ? (a, b) : (b, a);

        if (longest.ContainsPeriod(shortest)) return shortest.Duration;

        var count = longest.ContainsDate(shortest.Start)
            ? longest._finish.DayNumber - shortest.Start.DayNumber + 1
            : shortest.Finish.DayNumber - longest.Start.DayNumber + 1;
        
        return count;
    }
}

public enum PeriodComparerTypeEnum
{
    Start,
    Finish,
    Duration
}