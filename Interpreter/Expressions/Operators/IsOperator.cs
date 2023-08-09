using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record IsOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal IsOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

        if (right is IPattern pattern)
            return new Bool(pattern.GetRoot().Matches(left, call));

        throw new Throw($"Cannot apply operator 'is' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}