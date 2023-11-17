using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Reference : Value
{
    internal VariablePointer Pointer { get; }

    internal Reference() : this(new(null)) { }

    internal Reference(VariablePointer pointer)
    {
        Pointer = pointer;
    }

    public override ValueType GetType() => ValueType.Reference;
    public override string ToString() => "<reference>";

    internal static Reference Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Reference reference] => reference,
            [_] => throw new Throw($"'reference' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'reference' does not have a constructor that takes {values.Count} arguments")
        };
    }
}