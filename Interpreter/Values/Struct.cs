using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Values;

public sealed class Struct : Value, IIndexable
{
    private bool _assigned = false;

    internal Dictionary<string, IValue> Values { get; private set; }

    public Struct() => Values = new();

    public Struct(Dictionary<string, Value> values)
    {
        Values = values.ToDictionary(x => x.Key, x => (IValue)x.Value);
    }

    internal override ValueType GetType() => ValueType.Struct;

    internal static Struct Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Struct @struct] => @struct,
            [_] => throw new Throw($"'struct' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'struct' does not have a constructor that takes {values.Count} arguments")
        };
    }

    internal override void Destroy()
    {
        while (Values.Any())
            ((StructVariable)Values.First().Value).Delete();
    }

    internal override Value Copy(bool assign)
    {
        var @struct = new Struct();

        if (assign)
        {
            @struct._assigned = true;
            @struct.Values = Values
                .ToDictionary(x => x.Key, x => (IValue)new StructVariable(x.Key, x.Value.Value.GetOrCopy(assign), @struct));
        }
        else
        {
            @struct._assigned = false;
            @struct.Values = Values
                .ToDictionary(x => x.Key, x => (IValue)x.Value.Value.GetOrCopy(assign));
        }

        return @struct;
    }

    internal override Value GetOrCopy(bool assign)
    {
        var @struct = _assigned
            ? new Struct()
            : this;

        if (assign)
        {
            @struct._assigned = true;
            @struct.Values = Values
                .ToDictionary(x => x.Key, x => (IValue)new StructVariable(x.Key, x.Value.Value.GetOrCopy(assign), @struct));
        }
        else
        {
            @struct._assigned = false;
            @struct.Values = Values
                .ToDictionary(x => x.Key, x => (IValue)x.Value.Value.GetOrCopy(assign));
        }

        return @struct;
    }

    public override string ToString()
    {
        return Values.Count > 0
            ? "{" + string.Join(", ", Values.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value.Value}")) + "}"
            : "{...}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Values.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not Struct @struct)
            return false;

        if (Values.Count != @struct.Values.Count)
            return false;

        foreach (var key in Values.Keys)
        {
            if (!@struct.Values.TryGetValue(key, out var value))
                return false;

            if (Values[key].Value != value.Value)
                return false;
        }

        return true;
    }

    public IValue Index(Value index, Call _)
    {
        if (index is not String @string)
            throw new Throw("It should be a string.");

        if (!Values.ContainsKey(@string.Value))
            throw new Throw($"'{@string.Value}' was not defined inside this struct");

        if (_assigned)
            return new VariablePointer((StructVariable)Values[@string.Value]);
        else
            return Values[@string.Value];
    }

    public IValue Get(string key)
    {
        if (!Values.ContainsKey(key))
            throw new Throw($"'{key}' was not defined inside this struct");

        if (_assigned)
            return new VariablePointer((StructVariable)Values[key]);
        else
            return Values[key];
    }
}