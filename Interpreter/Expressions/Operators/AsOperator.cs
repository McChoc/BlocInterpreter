using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record AsOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal AsOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        right = ReferenceHelper.Resolve(right, call.Engine.Options.HopLimit).Value;

        if (left is Void)
            throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");

        var values = new List<Value>() { left.GetOrCopy() };

        if (right is not Type type)
            throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");

        try
        {
            return type.Value switch
            {
                ValueType.Void      => Void.Construct(values),
                ValueType.Null      => Null.Construct(values),
                ValueType.Bool      => Bool.Construct(values),
                ValueType.Number    => Number.Construct(values),
                ValueType.Range     => Range.Construct(values),
                ValueType.String    => String.Construct(values),
                ValueType.Array     => Array.Construct(values, call),
                ValueType.Struct    => Struct.Construct(values, call),
                ValueType.Tuple     => Tuple.Construct(values, call),
                ValueType.Func      => Func.Construct(values, call),
                ValueType.Task      => Task.Construct(values, call),
                ValueType.Iter      => Iter.Construct(values, call),
                ValueType.Reference => Reference.Construct(values),
                ValueType.Extern    => Extern.Construct(values),
                ValueType.Type      => Type.Construct(values),
                ValueType.Pattern   => Pattern.Construct(values),

                _ => throw new System.Exception()
            };
        }
        catch (Throw)
        {
            return Null.Value;
        }
    }
}