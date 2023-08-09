using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

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
            INumeric numeric => ComplementNumeric(numeric),
            IPattern pattern => ComplementPattern(pattern),

            _ => throw new Throw($"Cannot apply operator '~~' on type {value.GetTypeName()}")
        };
    }

    private static (Number, Number) ComplementNumeric(INumeric numeric)
    {
        var original = new Number(numeric.GetInt());
        var modified = new Number(~numeric.GetInt());

        return (original, modified);
    }

    private static (Pattern, Pattern) ComplementPattern(IPattern pattern)
    {
        var original = new Pattern(pattern.GetRoot());
        var modified = new Pattern(new NotPattern(pattern.GetRoot()));

        return (original, modified);
    }
}