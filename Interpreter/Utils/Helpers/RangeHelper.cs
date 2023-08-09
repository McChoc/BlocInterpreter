using Bloc.Values.Types;

namespace Bloc.Utils.Helpers;

internal static class RangeHelper
{
    private const double infinity = double.PositiveInfinity;

    internal static (double, double, double) Deconstruct(Range range)
    {
        double start = range.Start ?? (range.Step is null or > 0 ? 0 : -1);

        double end = range.End ?? (range.Step is null or > 0 ? infinity : -infinity);

        double step = range.Step ?? 1;

        return (start, end, step);
    }

    internal static (int, int, int) Deconstruct(Range range, int count)
    {
        int start = range switch
        {
            { Start: int n and >= 0 } => n,
            { Start: int n and < 0 } => count + n,
            { Start: null, Step: null or >= 0 } => 0,
            { Start: null, Step: < 0 } => count - 1,
        };

        int end = range switch
        {
            { End: int n and >= 0 } => n,
            { End: int n and < 0 } => count + n,
            { End: null, Step: null or >= 0 } => count,
            { End: null, Step: < 0 } => -1,
        };

        int step = range.Step ?? 1;

        return (start, end, step);
    }
}