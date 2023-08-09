using Bloc.Memory;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Expressions.Operators;

internal sealed record RvalOperator : IExpression
{
    private readonly IExpression _operand;

    internal RvalOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call);

        return ReferenceHelper.ResolveRecursive(value.Value, call.Engine.Options.HopLimit);
    }
}