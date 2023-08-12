namespace Bloc.Tokens;

internal sealed class NumberToken : Token
{
    public double Number { get; }

    internal NumberToken(int start, int end, double number)
        : base (start, end)
    {
        Number = number;
    }
}