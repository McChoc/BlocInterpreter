using System;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using String = Bloc.Values.String;

namespace Bloc.Expressions.Operators;

internal sealed record ComparisonOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal ComparisonOperator(IExpression left, IExpression right)
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

        if (left is String leftString && right is String rightString)
            return new Number(Math.Sign(string.CompareOrdinal(leftString.Value, rightString.Value)));

        if (left is INumeric leftScalar && right is INumeric rightScalar)
        {
            if (double.IsNaN(leftScalar.GetDouble()) || double.IsNaN(rightScalar.GetDouble()))
                return new Number(double.NaN);

            return new Number(Math.Sign(leftScalar.GetDouble() - rightScalar.GetDouble()));
        }

        throw new Throw($"Cannot apply operator '<=>' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}