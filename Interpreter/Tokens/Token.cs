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

    public abstract override int GetHashCode();

    public abstract override bool Equals(object other);

    public static bool operator ==(Token a, Token b) => a.Equals(b);

    public static bool operator !=(Token a, Token b) => !a.Equals(b);
}