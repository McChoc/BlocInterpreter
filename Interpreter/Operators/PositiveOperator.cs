using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record PositiveOperator : IExpression
{
    private readonly IExpression _operand;

    internal PositiveOperator(IExpression operand)
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
        if (value is IScalar scalar)
            return new Number(scalar.GetDouble());

        throw new Throw($"Cannot apply operator '+' on type {value.GetTypeName()}");
    }
}