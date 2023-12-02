namespace Bloc.Utils.Helpers;

internal static class NumberHelper
{
    internal static int Round(double n)
    {
        if (double.IsNaN(n))
            return 0;

        if (n > int.MaxValue)
            return int.MaxValue;

        if (n < int.MinValue)
            return int.MinValue;

        return (int)n;
    }
}