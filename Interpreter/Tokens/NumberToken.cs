using System;

namespace Bloc.Tokens;

internal sealed class NumberToken : Token
{
    public double Number { get; }

    internal NumberToken(int start, int end, double number)
        : base (start, end)
    {
        Number = number;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Number);
    }

    public override bool Equals(object other)
    {
        return other is NumberToken token &&
            Number == token.Number;
    }
}