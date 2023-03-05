using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record ConditionalOperator : IExpression
{
    private readonly IExpression _alternative;
    private readonly IExpression _condition;
    private readonly IExpression _consequent;

    internal ConditionalOperator(IExpression condition, IExpression consequent, IExpression alternative)
    {
        _condition = condition;
        _consequent = consequent;
        _alternative = alternative;
    }

    public IValue Evaluate(Call call)
    {
        var value = _condition.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        var @bool = Bool.ImplicitCast(value);

        return @bool.Value
            ? _consequent.Evaluate(call).Value
            : _alternative.Evaluate(call).Value;
    }
}