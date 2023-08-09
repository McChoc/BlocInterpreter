using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record IncrementPrefix : IExpression
{
    private readonly IExpression _operand;

    internal IncrementPrefix(IExpression operand)
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
        if (value is not INumeric numeric)
            throw new Throw($"Cannot apply operator '++' on type {value.GetTypeName()}");

        var number = new Number(numeric.GetDouble() + 1);

        return (number, number);
    }
}