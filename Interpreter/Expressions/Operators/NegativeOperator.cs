using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record NegativeOperator : IExpression
{
    private readonly IExpression _operand;

    internal NegativeOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _­operand.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(value, Operation, call);
    }

    private static Value Operation(Value value)
    {
        if (value is INumeric numeric)
            return new Number(-numeric.GetDouble());

        throw new Throw($"Cannot apply operator '-' on type {value.GetTypeName()}");
    }
}