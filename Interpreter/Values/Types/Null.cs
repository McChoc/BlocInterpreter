using System.Collections.Generic;
using Bloc.Results;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

public sealed class Null : Value
{
    public static Null Value { get; } = new();

    private Null() { }

    internal override ValueType GetType() => ValueType.Null;

    internal static Null Construct(List<Value> values)
    {
        return values switch
        {
            [] => Value,
            [..] => throw new Throw($"'null' does not have a constructor that takes {values.Count} arguments"),
        };
    }

    public override string ToString()
    {
        return "null";
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object other)
    {
        return other is Null;
    }
}