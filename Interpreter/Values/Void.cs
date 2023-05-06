using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Void : Value
{
    public static Void Value { get; } = new();

    private Void() { }

    internal override ValueType GetType() => ValueType.Void;

    internal static Void Construct(List<Value> values)
    {
        return values switch
        {
            [] => Value,
            [..] => throw new Throw($"'void' does not have a constructor that takes {values.Count} arguments"),
        };
    }

    public override Value Copy(bool assign)
    {
        if (assign)
            throw new Throw("'void' is not assignable");

        return this;
    }

    public override Value GetOrCopy(bool assign)
    {
        if (assign)
            throw new Throw("'void' is not assignable");

        return this;
    }

    public override string ToString()
    {
        return "void";
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object other)
    {
        return other is Void;
    }
}