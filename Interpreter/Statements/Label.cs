namespace Bloc.Statements;

internal sealed record Label
{
    internal int Index { get; }

    internal int Count { get; set; }

    internal Label(int index)
    {
        Index = index;
        Count = 0;
    }
}