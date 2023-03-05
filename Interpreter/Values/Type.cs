using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Type : Value, IInvokable
{
    public HashSet<ValueType> Value { get; }

    public Type() => Value = new();

    public Type(ValueType type) => Value = new() { type };

    public Type(HashSet<ValueType> types) => Value = types;

    internal override ValueType GetType() => ValueType.Type;

    internal static Type Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Type type] => type,
            [_] => throw new Throw($"'type' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'type' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override string ToString()
    {
        return "[type]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value.Count);
    }

    public override bool Equals(object other)
    {
        return other is Type type &&
            Value.SetEquals(type.Value);
    }

    public Value Invoke(List<Value> args, Dictionary<string, Value> kwargs, Call call)
    {
        if (Value.Count != 1)
            throw new Throw("Cannot instantiate a composite type");

        if (kwargs.Count != 0)
            throw new Throw("Constructors do not support named parameters");

        return Value.First() switch
        {
            ValueType.Void => Void.Construct(args),
            ValueType.Null => Null.Construct(args),
            ValueType.Bool => Bool.Construct(args),
            ValueType.Number => Number.Construct(args),
            ValueType.Range => Range.Construct(args),
            ValueType.String => String.Construct(args),
            ValueType.Array => Array.Construct(args),
            ValueType.Struct => Struct.Construct(args),
            ValueType.Tuple => Tuple.Construct(args),
            ValueType.Func => Func.Construct(args),
            ValueType.Task => Task.Construct(args, call),
            ValueType.Iter => Iter.Construct(args, call),
            ValueType.Reference => Reference.Construct(args, call),
            ValueType.Extern => Extern.Construct(args),
            ValueType.Type => Construct(args),
            _ => throw new Exception()
        };
    }
}