using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record CharacterOperator : IExpression
{
    private readonly IExpression _operand;

    internal CharacterOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is not INumeric scalar)
            throw new Throw($"Cannot apply operator 'chr' on type {value.GetTypeName()}");

        return new String(((char)scalar.GetInt()).ToString());
    }
}