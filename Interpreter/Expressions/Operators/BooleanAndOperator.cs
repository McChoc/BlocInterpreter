using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record BooleanAndOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal BooleanAndOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var value = _left.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        var @bool = Bool.ImplicitCast(value);

        if (!@bool.Value)
            return value;

        return _right.Evaluate(call).Value;
    }
}