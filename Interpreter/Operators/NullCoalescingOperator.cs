using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record NullCoalescingOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal NullCoalescingOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var value = _left.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is Void)
            throw new Throw("Cannot apply operator ?? to type 'void'");

        if (value is Null)
            return _right.Evaluate(call).Value;

        return value;
    }
}
