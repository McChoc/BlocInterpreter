namespace Bloc.Tokens;

internal abstract class Token
{
    internal int Start { get; }
    internal int End { get; }

    internal Token(int start, int end)
    {
        Start = start;
        End = end;
    }
}