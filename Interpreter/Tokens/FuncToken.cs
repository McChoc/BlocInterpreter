using System;
using Bloc.Expressions;

namespace Bloc.Tokens;

internal sealed class FuncToken : Token
{
    public FuncLiteral Literal { get; set; }

    internal FuncToken(int start, int end, FuncLiteral literal)
        : base(start, end)
    {
        Literal = literal;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Literal);
    }

    public override bool Equals(object other)
    {
        return other is FuncToken token &&
            Literal.Equals(token.Literal);
    }
}