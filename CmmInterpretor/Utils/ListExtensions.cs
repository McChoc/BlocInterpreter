using System;
using System.Collections.Generic;

namespace CmmInterpretor.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> Split<T>(this List<T> list, T separator)
        {
            var result = new List<List<T>>();

            int start = 0;

            for (int i = 0; i <= list.Count; i++)
            {
                if (i == list.Count || list[i].Equals(separator))
                {
                    result.Add(list.GetRange(start..i));
                    start = i + 1;
                }
            }

            return result;
        }

        public static List<List<T>> Split<T>(this List<T> list, Func<T,bool> predicate)
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

        public static List<T> GetRange<T>(this List<T> list, Range range)
        {
            var (start, length) = range.GetOffsetAndLength(list.Count);
            return list.GetRange(start, length);
        }
    }
}
