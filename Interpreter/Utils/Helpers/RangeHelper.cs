using System;
using Range = Bloc.Values.Types.Range;

namespace Bloc.Utils.Helpers;

internal static class RangeHelper
{
    internal static (double, double, double) GetLoopParameters(Range range)
    {
        double start = range.Start.Value ?? (range.Step < 0 ? -0.0 : 0.0);
        double stop = range.Stop.Value ?? (range.Step < 0 ? double.NegativeInfinity : double.PositiveInfinity);
        double step = range.Step ?? 1;

        return (start, stop, step);
    }

    internal static (int, double, double) GetSliceParameters(Range range, int length)
    {
        double start = range.Start.Value ?? (range.Step < 0 ? double.PositiveInfinity : double.NegativeInfinity);
        double stop = range.Stop.Value ?? (range.Step < 0 ? double.NegativeInfinity : double.PositiveInfinity);
        double step = range.Step ?? 1;

        if (start < 0)
            start += length;

        if (stop < 0)
            stop += length;

        if ((step > 0 && stop < 0) || (step < 0 && start < 0))
            return default;

        bool excludeStart = !range.Start.Inclusive && start >= 0 && start < length;
        bool excludeStop = !range.Stop.Inclusive && stop >= 0 && stop < length;

        start = Math.Clamp(start, 0, length);
        stop = Math.Clamp(stop, 0, length);

        int count = (int)Math.Ceiling((stop - start) / step);

        double nextIndex = start + count * step;

        if (step > 0)
        {
            if (nextIndex < stop || (nextIndex == stop && !excludeStop && stop != length))
                count++;
        }
        else
        {
            if (nextIndex > stop || (nextIndex == stop && !excludeStop && start != length))
                count++;

            if (start == length)
                start += Math.Max(step, -1);
        }

        if (excludeStop && start + (count - 1) * step == stop)
            count--;

        if (excludeStart)
        {
            start += step;
            count--;
        }

        return (count, start, step);
    }

    internal static bool Contains(Range range, double n)
    {
        if (double.IsNaN(n))
            return false;

        return range.Step is null
            ? ContinuousContains(range, n)
            : DiscreteContains(range, n);
    }

    private static bool ContinuousContains(Range range, double n)
    {
        double start = range.Start.Value ?? double.NegativeInfinity;
        double stop = range.Stop.Value ?? double.PositiveInfinity;

        if (double.IsNaN(start) || double.IsNaN(stop))
            return false;

        if (range.Start.Inclusive && n < start)
            return false;

        if (!range.Start.Inclusive && n <= start)
            return false;

        if (range.Stop.Inclusive && n > stop)
            return false;

        if (!range.Stop.Inclusive && n >= stop)
            return false;

        return true;
    }

    private static bool DiscreteContains(Range range, double n)
    {
        var (start, stop, step) = GetLoopParameters(range);

        if (double.IsNaN(start) || double.IsNaN(stop) || double.IsNaN(step) || double.IsInfinity(step) || step == 0)
            return false;

        double index = (n - start) / step;
        double length = (stop - start) / step;

        if (index % 1 != 0)
            return false;

        if (range.Start.Inclusive && index < 0)
            return false;

        if (!range.Start.Inclusive && index <= 0)
            return false;

        if (range.Stop.Inclusive && index > length)
            return false;

        if (!range.Stop.Inclusive && index >= length)
            return false;

        return true;
    }
}