using System;
using Range = Bloc.Values.Types.Range;

namespace Bloc.Utils.Helpers;

internal static class RangeHelper
{
    internal static (double, double, int) Deconstruct(Range range)
    {
        int step = range.Step ?? 1;

        double start = range.Start ?? (step < 0 ? -1 : 0);
        double stop = range.Stop ?? (step < 0 ? double.NegativeInfinity : double.PositiveInfinity);

        if (range.Inclusive)
            stop += step < 0 ? -1 : 1;

        return (start, stop, step);
    }

    internal static (int, int, int) Deconstruct(Range range, int count)
    {
        int step = range.Step ?? 1;

        int upper = step < 0 ? count - 1 : count;
        int lower = step < 0 ? -1 : 0;

        int start = range.Start ?? (step < 0 ? upper : lower);
        int stop = range.Stop ?? (step < 0 ? lower : upper);

        if (start < 0)
            start += count;

        if (stop < 0)
            stop += count;

        if (range.Inclusive)
            stop += step < 0 ? -1 : 1;

        start = Math.Clamp(start, lower, upper);
        stop = Math.Clamp(stop, lower, upper);

        return (start, stop, step);
    }
}