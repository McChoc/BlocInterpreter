using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record TypeofOperator : IExpression
{
    private readonly IExpression _operand;

    internal TypeofOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        return new Type(value.GetType());
    }
}