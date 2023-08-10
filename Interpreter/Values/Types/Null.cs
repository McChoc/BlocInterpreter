using System.Collections.Generic;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Null : Value
{
    public static Null Value { get; } = new();

    private Null() { }

    public override ValueType GetType() => ValueType.Null;
    public override string ToString() => "null";

    internal static Null Construct(List<Value> values)
    {
        return values switch
        {
            [] => Value,
            [..] => throw new Throw($"'null' does not have a constructor that takes {values.Count} arguments"),
        };
    }
}