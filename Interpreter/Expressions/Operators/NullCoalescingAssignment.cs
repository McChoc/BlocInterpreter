using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NullCoalescingAssignment : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal NullCoalescingAssignment(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var value = _left.Evaluate(call);

        if (value is not Pointer pointer)
            throw new Throw("The value must be assignable");

        if (value.Value is not Null)
            return value.Value;

        value = _right.Evaluate(call);
        return pointer.Set(value.Value);
    }
}