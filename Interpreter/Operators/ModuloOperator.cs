using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record ModuloOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal ModuloOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(left, right, Operation, call);
    }

    internal static Value Operation(Value a, Value b)
    {
        return (a, b) switch
        {
            (IScalar left, IScalar right) => ModScalars(left, right),

            _ => throw new Throw($"Cannot apply operator '%%' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number ModScalars(IScalar left, IScalar right)
    {
        var dividend = left.GetDouble();
        var divisor = right.GetDouble();

        return new Number((dividend % divisor + divisor) % divisor);
    }
}