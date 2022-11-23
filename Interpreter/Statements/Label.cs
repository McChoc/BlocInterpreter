namespace Bloc.Statements
{
    internal sealed record Label
    {
        internal Label(int index)
        {
            Index = index;
            Count = 0;
        }

        internal int Index { get; }

        internal int Count { get; set; }
    }
}