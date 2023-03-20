using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record ComplementPrefix : IExpression
{
    private readonly IExpression _operand;

    internal ComplementPrefix(IExpression operand)
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
        var number = new Number(~scalar.GetInt());

        return (number, number);
    }

    private static (Type, Type) ComplementType(Type value)
    {
        var types = new HashSet<ValueType>();

        foreach (ValueType type in System.Enum.GetValues(typeof(ValueType)))
            if (!value.Value.Contains(type))
                types.Add(type);

        value = new Type(types);

        return (value, value);
    }
}