using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record NotEqualOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal NotEqualOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        return new Bool(!left.Equals(right));
    }
}