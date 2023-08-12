using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record ChrOperator : IExpression
{
    private readonly IExpression _operand;

    internal ChrOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not INumeric numeric)
            throw new Throw($"Cannot apply operator 'chr' on type {value.GetTypeName()}");

        return new String(((char)numeric.GetInt()).ToString());
    }
}