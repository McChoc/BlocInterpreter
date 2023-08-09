using System;
using Bloc.Expressions;

namespace Bloc.Tokens;

internal sealed class ParsedToken : Token
{
    public IExpression Expression { get; set; }

    internal ParsedToken(int start, int end, IExpression expression)
        : base(start, end)
    {
        Expression = expression;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Expression);
    }

    public override bool Equals(object other)
    {
        return other is ParsedToken token &&
            Expression.Equals(token.Expression);
    }
}