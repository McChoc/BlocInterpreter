using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Reference : Value
{
    internal Pointer Pointer { get; }

    internal Reference() => Pointer = new VariablePointer(null);

    internal Reference(Pointer pointer) => Pointer = pointer;

    internal override ValueType GetType() => ValueType.Reference;

    internal static Reference Construct(List<Value> values, Call call)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Reference reference] => reference,
            [String @string] => new(call.Get(@string.Value).Resolve()),
            [_] => throw new Throw($"'reference' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'reference' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override string ToString()
    {
        return "[reference]";
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object other)
    {
        return other is Reference reference &&
            Pointer == reference.Pointer;
    }
}