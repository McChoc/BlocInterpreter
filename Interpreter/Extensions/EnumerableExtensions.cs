using System.Collections.Generic;
using System.Linq;

namespace Bloc.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T, U)> Zip<T, U>(this IEnumerable<T> a, IEnumerable<U> b)
        {
            return a.Zip(b, (a, b) => (a, b));
        }
    }
}