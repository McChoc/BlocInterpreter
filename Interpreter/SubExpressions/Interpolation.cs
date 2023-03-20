using System;
using Bloc.Expressions;

namespace Bloc.SubExpressions;

internal sealed class Interpolation
{
    public int Index { get; }
    public IExpression Expression { get; }

    public Interpolation(int index, IExpression expression)
    {
        Index = index;
        Expression = expression;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Index, Expression);
    }

    public override bool Equals(object other)
    {
        return other is Interpolation interpolation &&
            Index == interpolation.Index &&
            Expression.Equals(interpolation.Expression);
    }
}
