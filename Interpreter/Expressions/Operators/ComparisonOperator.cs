using System;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;
using String = Bloc.Values.Types.String;

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

        left = ReferenceHelper.Resolve(left, call.Engine.Options.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

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