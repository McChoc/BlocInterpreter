using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Type = Bloc.Values.Type;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Operators;

internal sealed record BitwiseAndOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal BitwiseAndOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(left, right, Operation, call);
    }

    internal static Value Operation(Value a, Value b)
    {
        return (a, b) switch
        {
            (IScalar left, IScalar right)   => AndScalars(left, right),
            (Type left, Type right)         => AndTypes(left, right),

            _ => throw new Throw($"Cannot apply operator '&' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number AndScalars(IScalar left, IScalar right)
    {
        return new Number(left.GetInt() & right.GetInt());
    }

    private static Type AndTypes(Type left, Type right)
    {
        var types = new HashSet<ValueType>();

        foreach (ValueType type in Enum.GetValues(typeof(ValueType)))
            if (left.Value.Contains(type) && right.Value.Contains(type))
                types.Add(type);

        return new Type(types);
    }
}