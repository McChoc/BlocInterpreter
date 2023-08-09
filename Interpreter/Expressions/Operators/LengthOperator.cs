using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record LengthOperator : IExpression
{
    private readonly IExpression _operand;

    internal LengthOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is Array array)
            return new Number(array.Values.Count);

        if (value is String @string)
            return new Number(@string.Value.Length);

        throw new Throw($"Cannot apply operator 'len' type {value.GetTypeName()}");
    }
}