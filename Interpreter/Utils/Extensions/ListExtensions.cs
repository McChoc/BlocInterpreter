using System;
using System.Collections.Generic;

namespace Bloc.Utils.Extensions;

public static class ListExtensions
{
    public static List<T> GetRange<T>(this List<T> list, Range range)
    {
        var (start, length) = range.GetOffsetAndLength(list.Count);
        return list.GetRange(start, length);
    }

    public static List<List<T>> Split<T>(this List<T> list, T separator)
    {
        if (separator is null)
            throw new ArgumentNullException(nameof(separator));

        var result = new List<List<T>>();

        int start = 0;

        for (int i = 0; i <= list.Count; i++)
        {
            if (i == list.Count || separator.Equals(list[i]))
            {
                result.Add(list.GetRange(start..i));
                start = i + 1;
            }
        }

        return result;
    }

    public static List<List<T>> Split<T>(this List<T> list, Predicate<T> predicate)
    {
        var result = new List<List<T>>();

        int start = 0;

        for (int i = 0; i <= list.Count; i++)
        {
            if (i == list.Count || predicate(list[i]))
            {
                result.Add(list.GetRange(start..i));
                start = i + 1;
            }
        }

        return result;
    }
}