using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record GroupbyOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal GroupbyOperator(IExpression left, IExpression right)
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
                .Select(x => x.Value.GetOrCopy())
                .GroupBy(x => func.Invoke(new() { x }, new(), call), (key, values) => new Tuple(new List<Value>() { key, new Array(values.ToList()) }))
                .ToList<Value>());

        throw new Throw($"Cannot apply operator 'groupby' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }
}