using System;
using Range = Bloc.Values.Types.Range;

namespace Bloc.Utils.Helpers;

internal static class RangeHelper
{
    private const double INFINITY = double.PositiveInfinity;

    internal static (double, double, double) Deconstruct(Range range)
    {
        double start = range.Start ?? (range.Step is null or > 0 ? 0 : -1);

        double end = range.End ?? (range.Step is null or > 0 ? INFINITY : -INFINITY);

        double step = range.Step ?? 1;

        return (start, end, step);
    }

    internal static (int, int, int) Deconstruct(Range range, int count)
    {
        int start = range switch
        {
            { Start: int n and >= 0 } => Math.Clamp(n, 0, count - 1),
            { Start: int n and < 0 } => Math.Clamp(count + n, 0, count - 1),
            { Start: null, Step: null or >= 0 } => 0,
            { Start: null, Step: < 0 } => count - 1,
        };

        int end = range switch
        {
            { End: int n and >= 0 } => Math.Clamp(n, -1, count),
            { End: int n and < 0 } => Math.Clamp(count + n, -1, count),
            { End: null, Step: null or >= 0 } => count,
            { End: null, Step: < 0 } => -1,
        };

        int step = range.Step ?? 1;

        return (start, end, step);
    }
}