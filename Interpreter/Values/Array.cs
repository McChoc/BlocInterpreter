using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Variables;

namespace Bloc.Values;

public sealed class Array : Value, IIndexable
{
    private bool _assigned = false;

    internal List<IValue> Values { get; private set; }

    public Array() => Values = new();

    public Array(List<Value> values)
    {
        Values = values.ToList<IValue>();
    }

    internal override ValueType GetType() => ValueType.Array;

    internal static Array Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),
            [Array array] => array,
            [Number number] => new(Enumerable.Repeat((Value)Null.Value, number.GetInt()).ToList()),
            [String @string] => new(@string.Value.ToCharArray().Select(x => (Value)new String(x.ToString())).ToList()),
            [Struct @struct] => new(@struct.Values.OrderBy(x => x.Key).Select(x => (Value)new Tuple(new List<Value>() { new String(x.Key), x.Value.Value })).ToList()),
            [Tuple tuple] => new(tuple.Values.Select(x => x.Value).ToList()),
            [Iter iter] => new(iter.Iterate().ToList()),
            [Type type] => new(type.Value.Select(x => (Value)new Type(x)).ToList()),
            [var value, Number number] => new(Enumerable.Repeat(value, number.GetInt()).ToList()),
            [_] => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [_, _] => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [..] => throw new Throw($"'array' does not have a constructor that takes {values.Count} arguments")
        };
    }

    internal override void Destroy()
    {
        while (Values.Count > 0)
            ((ArrayVariable)Values.Last()).Delete();
    }

    internal override Value Copy(bool assign)
    {
        var array = new Array();

        if (assign)
        {
            array._assigned = true;
            array.Values = Values
                .Select(x => new ArrayVariable(x.Value.GetOrCopy(assign), array))
                .ToList<IValue>();
        }
        else
        {
            array._assigned = false;
            array.Values = Values
                .Select(x => x.Value.GetOrCopy(assign))
                .ToList<IValue>();
        }

        return array;
    }

    internal override Value GetOrCopy(bool assign)
    {
        var array = _assigned
            ? new Array()
            : this;

        if (assign)
        {
            array._assigned = true;
            array.Values = Values
                .Select(x => new ArrayVariable(x.Value.GetOrCopy(assign), array))
                .ToList<IValue>();
        }
        else
        {
            array._assigned = false;
            array.Values = Values
                .Select(x => x.Value.GetOrCopy(assign))
                .ToList<IValue>();
        }

        return array;
    }

    public override string ToString()
    {
        return Values.Count > 0
            ? "{" + string.Join(", ", Values.Select(v => v.Value)) + "}"
            : "{..}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Values.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not Array array)
            return false;

        if (Values.Count != array.Values.Count)
            return false;

        for (var i = 0; i < Values.Count; i++)
            if (Values[i].Value != array.Values[i].Value)
                return false;

        return true;
    }

    public IValue Index(Value value, Call call)
    {
        if (value is Number number)
        {
            var index = number.GetInt();

            if (index < 0)
                index += Values.Count;

            if (index < 0 || index >= Values.Count)
                throw new Throw("Index out of range");

            if (_assigned)
                return new VariablePointer((ArrayVariable)Values[index]);
            else
                return Values[index];
        }

        if (value is Range range)
        {
            if (_assigned)
                return new SlicePointer(this, range);

            var (start, end, step) = RangeHelper.Deconstruct(range, Values.Count);

            var values = new List<Value>();

            for (int i = start; i != end && end - i > 0 == step > 0; i += step)
            {
                if (i < 0 || i >= Values.Count)
                    throw new Throw("Index out of range");

                values.Add(Values[i].Value);
            }

            return new Array(values);
        }

        throw new Throw("It should be a number or a range.");
    }
}