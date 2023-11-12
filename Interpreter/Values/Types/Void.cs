using System.Collections.Generic;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Void : Value
{
    public static Void Value { get; } = new();

    private Void() { }

    public override ValueType GetType() => ValueType.Void;
    public override string ToString() => "void";

    public override Value Copy(bool assign) => GetOrCopy(assign);

    public override Value GetOrCopy(bool assign)
    {
        if (assign)
            throw new Throw("'void' is not assignable");

        return this;
    }

    internal static Void Construct(List<Value> values)
    {
        return values switch
        {
            [] => Value,
            [..] => throw new Throw($"'void' does not have a constructor that takes {values.Count} arguments"),
        };
    }
}