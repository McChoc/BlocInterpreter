namespace Bloc.Tokens;

internal abstract class Token : IToken
{
    public int Start { get; }
    public int End { get; }

    internal Token(int start, int end)
    {
        Start = start;
        End = end;
    }
}