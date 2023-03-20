using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record RemainderOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal RemainderOperator(IExpression left, IExpression right)
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
            (INumeric left, INumeric right) => RemScalars(left, right),

            _ => throw new Throw($"Cannot apply operator '%' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number RemScalars(INumeric left, INumeric right)
    {
        return new Number(left.GetDouble() % right.GetDouble());
    }
}