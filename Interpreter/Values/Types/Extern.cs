using System.Collections.Generic;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Extern : Value
{
    public object? Value { get; }

    public Extern(object? value = null)
    {
        Value = value;
    }

    public override ValueType GetType() => ValueType.Extern;
    public override string ToString() => "[extern]";

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
}