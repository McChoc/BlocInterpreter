using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NegationPostfix : IExpression
{
    private readonly IExpression _operand;

    internal NegationPostfix(IExpression operand)
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
        var @bool = Bool.ImplicitCast(value);

        return (@bool, new Bool(!@bool.Value));
    }
}