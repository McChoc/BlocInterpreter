using System;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Void = Bloc.Values.Void;
using Range = Bloc.Values.Range;
using String = Bloc.Values.String;
using Array = Bloc.Values.Array;
using Tuple = Bloc.Values.Tuple;
using Type = Bloc.Values.Type;
using ValueType = Bloc.Values.ValueType;
using System.Collections.Generic;

namespace Bloc.Operators;

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

        right = ReferenceHelper.Resolve(right, call.Engine.HopLimit).Value;

        if (left is Void)
            throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");

        var values = new List<Value>() { left.GetOrCopy() };

        if (right is not Type type)
            throw new Throw($"Cannot apply operator 'as' on operands of types {left.GetTypeName()} and {right.GetTypeName()}");

        if (type.Value.Count != 1)
            throw new Throw("Cannot apply operator 'as' on a composite type");

        try
        {
            return type.Value.First() switch
            {
                ValueType.Void      => Void.Construct(values),
                ValueType.Null      => Null.Construct(values),
                ValueType.Bool      => Bool.Construct(values),
                ValueType.Number    => Number.Construct(values),
                ValueType.Range     => Range.Construct(values),
                ValueType.String    => String.Construct(values),
                ValueType.Array     => Array.Construct(values),
                ValueType.Struct    => Struct.Construct(values),
                ValueType.Tuple     => Tuple.Construct(values),
                ValueType.Func      => Func.Construct(values),
                ValueType.Task      => Task.Construct(values, call),
                ValueType.Iter      => Iter.Construct(values, call),
                ValueType.Reference => Reference.Construct(values, call),
                ValueType.Extern    => Extern.Construct(values),
                ValueType.Type      => Type.Construct(values),

                _ => throw new Exception()
            };
        }
        catch (Throw)
        {
            return Null.Value;
        }
    }
}