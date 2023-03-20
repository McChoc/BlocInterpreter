using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record LessEqualOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal LessEqualOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        left = ReferenceHelper.Resolve(left, call.Engine.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.HopLimit).Value;

        if (left is INumeric leftScalar && right is INumeric rightScalar)
            return new Bool(leftScalar.GetDouble() <= rightScalar.GetDouble());

        if (left is String leftString && right is String rightString)
            return new Bool(string.CompareOrdinal(leftString.Value, rightString.Value) <= 0);

        throw new Throw($"Cannot apply operator '<=' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}