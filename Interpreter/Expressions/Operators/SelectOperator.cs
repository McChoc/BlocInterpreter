using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record SelectOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal SelectOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        left = ReferenceHelper.Resolve(left, call.Engine.Options.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

        if (left is Array array && right is Func func)
            return new Array(array.Values
                .Select(x => func.Invoke(new() { x.Value.GetOrCopy() }, new(), call))
                .ToList());

        throw new Throw($"Cannot apply operator 'select' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}