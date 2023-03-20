using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record IncrementPostfix : IExpression
{
    private readonly IExpression _operand;

    internal IncrementPostfix(IExpression operand)
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
        if (value is not INumeric scalar)
            throw new Throw($"Cannot apply operator '++' on type {value.GetTypeName()}");

        var original = new Number(scalar.GetDouble());
        var incremented = new Number(scalar.GetDouble() + 1);

        return (original, incremented);
    }
}