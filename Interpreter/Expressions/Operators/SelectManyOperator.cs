using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record SelectManyOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal SelectManyOperator(IExpression left, IExpression right)
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

        if (left is not Array array || right is not Func func)
            throw new Throw($"Cannot apply operator 'select ..' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");

        var values = new List<Value>();

        foreach (var value in array.Values)
        {
            var result = func.Invoke(new() { value.Value.GetOrCopy() }, new(), call);
            result = ReferenceHelper.Resolve(result, call.Engine.Options.HopLimit).Value;

            if (!Iter.TryImplicitCast(result, out var iter, call))
                throw new Throw("Cannot implicitly convert to iter");

            values.AddRange(IterHelper.CheckedIterate(iter, call.Engine.Options));
        }

        return new Array(values);
    }
}