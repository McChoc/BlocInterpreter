using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record OrdOperator : IExpression
{
    private readonly IExpression _operand;

    internal OrdOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not String @string)
            throw new Throw($"Cannot apply operator 'ord' on type {value.GetTypeName()}");

        if (@string.Value.Length != 1)
            throw new Throw("The string must contain exactly one character");

        return new Number(@string.Value[0]);
    }
}