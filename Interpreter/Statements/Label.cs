namespace Bloc.Statements
{
    internal class Label
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