using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record NegationOperator : IExpression
{
    private readonly IExpression _operand;

    internal NegationOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(value, Operation, call);
    }

    private static Value Operation(Value value)
    {
        var @bool = Bool.ImplicitCast(value);

        return new Bool(!@bool.Value);
    }
}