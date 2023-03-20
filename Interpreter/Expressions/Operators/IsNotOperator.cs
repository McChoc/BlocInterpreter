using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record IsNotOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal IsNotOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        right = ReferenceHelper.Resolve(right, call.Engine.HopLimit).Value;

        if (right is Type type)
            return new Bool(!type.Value.Contains(left.GetType()));

        throw new Throw($"Cannot apply operator 'is not' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}