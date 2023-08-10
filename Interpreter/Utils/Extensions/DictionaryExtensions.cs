using System;
using System.Collections.Generic;

namespace Bloc.Utils.Extensions;

public static class DictionaryExtensions
{
    public static bool KeyValueEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
    {
        if (first == null || second == null)
            throw new ArgumentNullException();

        if (first.Count != second.Count)
            return false;

        foreach (var (key, firstValue) in first)
            if (!second.TryGetValue(key, out var secondValue) || !Equals(firstValue, secondValue))
                return false;

        return true;
    }

    public static bool KeyValueEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> comparer)
    {
        if (first == null || second == null)
            throw new ArgumentNullException();

        if (first.Count != second.Count)
            return false;

        foreach (var (key, firstValue) in first)
            if (!second.TryGetValue(key, out var secondValue) || !comparer.Equals(firstValue, secondValue))
                return false;

        return true;
    }
}