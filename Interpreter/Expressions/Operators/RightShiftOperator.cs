using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record RightShiftOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal RightShiftOperator(IExpression left, IExpression right)
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
        if (a is INumeric left && b is INumeric right)
            return new Number(left.GetInt() >> right.GetInt());

        throw new Throw($"Cannot apply operator '>>' on operands of types {a.GetTypeName()} and {b.GetTypeName()}");
    }
}