using Bloc.Values;

namespace Bloc.Utils.Helpers;

internal static class RangeHelper
{
    internal static (int, int) GetStartAndEnd(Range range, int count)
    {
        int start = range switch
        {
            { Start: int n and >= 0 } => n,
            { Start: int n and < 0 } => count + n,
            { Start: null, Step: >= 0 } => 0,
            { Start: null, Step: < 0 } => count - 1,
        };

        int end = range switch
        {
            { End: int n and >= 0 } => n,
            { End: int n and < 0 } => count + n,
            { End: null, Step: >= 0 } => count,
            { End: null, Step: < 0 } => -1,
        };

        return (start, end);
    }
}