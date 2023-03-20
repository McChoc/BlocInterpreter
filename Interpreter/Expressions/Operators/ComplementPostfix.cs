using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Type = Bloc.Values.Type;
using ValueType = Bloc.Values.ValueType;

namespace Bloc.Expressions.Operators;

internal sealed record ComplementPostfix : IExpression
{
    private readonly IExpression _operand;

    internal ComplementPostfix(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call);

        return AdjustmentHelper.Adjust(value, Adjustment, call);
    }

    private static (Value, Value) Adjustment(Value value)
    {
        return value switch
        {
            INumeric scalar => ComplementScalar(scalar),
            Type type => ComplementType(type),

            _ => throw new Throw($"Cannot apply operator '~~' on type {value.GetTypeName()}")
        };
    }

    private static (Number, Number) ComplementScalar(INumeric scalar)
    {
        var original = new Number(scalar.GetInt());
        var modified = new Number(~scalar.GetInt());

        return (original, modified);
    }

    private static (Type, Type) ComplementType(Type value)
    {
        var types = new HashSet<ValueType>();

        foreach (ValueType type in Enum.GetValues(typeof(ValueType)))
            if (!value.Value.Contains(type))
                types.Add(type);

        return (value, new Type(types));
    }
}