using System;
using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Type = Bloc.Values.Type;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Expressions.Operators;

internal sealed record ComplementOperator : IExpression
{
    private readonly IExpression _operand;

    internal ComplementOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(value, Operation, call);
    }

    internal static Value Operation(Value value)
    {
        return value switch
        {
            INumeric scalar => ComplementScalar(scalar),
            Type type => ComplementType(type),

            _ => throw new Throw($"Cannot apply operator '~' on type {value.GetTypeName()}")
        };
    }

    private static Number ComplementScalar(INumeric scalar)
    {
        return new Number(~scalar.GetInt());
    }

    private static Type ComplementType(Type value)
    {
        var types = new HashSet<ValueType>();

        foreach (ValueType type in Enum.GetValues(typeof(ValueType)))
            if (!value.Value.Contains(type))
                types.Add(type);

        return new Type(types);
    }
}