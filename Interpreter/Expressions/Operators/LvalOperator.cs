using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record LvalOperator : IExpression
{
    private readonly IExpression _operand;

    internal LvalOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        if (value is not Reference)
            throw new Throw("The 'lval' operator can only be used on references");

        return ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit);
    }
}