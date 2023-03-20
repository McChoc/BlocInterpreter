using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NegationPrefix : IExpression
{
    private readonly IExpression _operand;

    internal NegationPrefix(IExpression operand)
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

        @bool = new Bool(!@bool.Value);

        return (@bool, @bool);
    }
}