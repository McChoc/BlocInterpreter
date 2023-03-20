using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record VoidCoalescingOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal VoidCoalescingOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var value = _left.Evaluate(call).Value;

        if (value is Void)
            return _right.Evaluate(call).Value;

        return value;
    }
}