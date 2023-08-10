namespace Bloc.Statements;

internal sealed record LabelInfo
{
    internal int Index { get; }
    internal int Count { get; set; }

    internal LabelInfo(int index)
    {
        Index = index;
        Count = 0;
    }
}