using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record BooleanXorOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal BooleanXorOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;

        left = ReferenceHelper.Resolve(left, call.Engine.Options.HopLimit).Value;

        var leftBool = Bool.ImplicitCast(left);

        var right = _right.Evaluate(call).Value;

        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

        var rightBool = Bool.ImplicitCast(right);

        if (leftBool.Value && !rightBool.Value)
            return left;

        if (!leftBool.Value && rightBool.Value)
            return right;

        return Null.Value;
    }
}