using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Array = Bloc.Values.Array;

namespace Bloc.Expressions.Operators;

internal sealed record OrderbyOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal OrderbyOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        left = ReferenceHelper.Resolve(left, call.Engine.HopLimit).Value;
        right = ReferenceHelper.Resolve(right, call.Engine.HopLimit).Value;

        if (left is Array array && right is Func func)
        {
            try
            {
                return new Array(array.Values
                    .Select(x => x.Value.GetOrCopy())
                    .OrderBy(x => x, new ValueComparer(func, call))
                    .ToList());
            }
            catch (InvalidOperationException e) when (e.InnerException is Throw t)
            {
                throw t;
            }
        }

        throw new Throw($"Cannot apply operator 'where' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");
    }

    private class ValueComparer : IComparer<Value>
    {
        private readonly Func _func;
        private readonly Call _call;

        public ValueComparer(Func func, Call call)
        {
            _func = func;
            _call = call;
        }

        public int Compare(Value a, Value b)
        {
            var value = _func.Invoke(new() { a.Copy(), b.Copy() }, new(), _call);

            return Number.ImplicitCast(value).GetInt();
        }
    }
}