using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Tuple : Value
{
    private bool _assigned = false;

    public List<IValue> Values { get; private set; }

    public Tuple() => Values = new();

    public Tuple(List<IValue> values)
    {
        Values = values;
    }

    public Tuple(List<Value> values)
    {
        Values = values.ToList<IValue>();
    }

    internal override ValueType GetType() => ValueType.Tuple;

    internal static Tuple Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),

            [Tuple tuple] => tuple,

            [Array array] => new(array.Values
                .Select(x => x.Value)
                .ToList()),

            [Struct @struct] => new(@struct.Values
                .OrderBy(x => x.Key)
                .Select(x => x.Value.Value)
                .ToList()),

            [Range range] => new(new List<Value>()
            {
                range.Start is int l ? new Number(l) : Null.Value,
                range.End is int m ? new Number(m) : Null.Value,
                range.Step is int n ? new Number(n) : Null.Value
            }),

            [_] => throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override Value Copy(bool assign)
    {
        return new Tuple
        {
            _assigned = assign,

            Values = Values
                .Select(x => x.Value.GetOrCopy(assign))
                .ToList<IValue>()
        };
    }

    public override Value GetOrCopy(bool assign)
    {
        var tuple = _assigned
            ? new Tuple()
            : this;

        tuple._assigned = assign;

        tuple.Values = Values
            .Select(x => x.Value.GetOrCopy(assign))
            .ToList<IValue>();

        return tuple;
    }

    public override string ToString()
    {
        return "(" + string.Join(", ", Values.Select(v => v.Value)) + ")";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Values.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not Tuple tuple)
            return false;

        if (Values.Count != tuple.Values.Count)
            return false;

        for (var i = 0; i < Values.Count; i++)
            if (Values[i].Value != tuple.Values[i].Value)
                return false;

        return true;
    }
}