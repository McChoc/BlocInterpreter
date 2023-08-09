using System;
using System.Collections.Generic;
using Bloc.Results;
using Bloc.Values.Core;
using ValueType = Bloc.Values.Core.ValueType;

namespace Bloc.Values.Types;

public sealed class Extern : Value
{
    public object? Value { get; }

    public Extern() => Value = null;

    public Extern(object? value) => Value = value;

    internal override ValueType GetType() => ValueType.Extern;

    internal static Extern Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Extern @extern] => @extern,
            [_] => throw new Throw($"'extern' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'extern' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override string ToString()
    {
        return "[extern]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public override bool Equals(object other)
    {
        return other is Extern @extern &&
            Value == @extern.Value;
    }
}