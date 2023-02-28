using Bloc.Values;

namespace Bloc.Utils.Helpers
{
    internal static class RangeUtil
    {
        internal static (int, int) GetStartAndEnd(Range range, int count)
        {
            var start = (int)(range.Start != null
                ? range.Start >= 0
                    ? range.Start
                    : count + range.Start
                : range.Step >= 0
                    ? 0
                    : count - 1);

            var end = (int)(range.End != null
                ? range.End >= 0
                    ? range.End
                    : count + range.End
                : range.Step >= 0
                    ? count
                    : -1);

            return (start, end);
        }
    }
}