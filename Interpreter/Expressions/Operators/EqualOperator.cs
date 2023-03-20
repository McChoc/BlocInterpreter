using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record EqualOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal EqualOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        return new Bool(left.Equals(right));
    }
}