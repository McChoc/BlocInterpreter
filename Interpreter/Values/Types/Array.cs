using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Comparers;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Array : Value, IIndexable, IPattern
{
    [DoNotCompare]
    private bool _assigned = false;

    [CompareUsing<ValueEqualityComparer>]
    internal List<IValue> Values { get; private set; }

    public Array() : this(new()) { }

    public Array(List<Value> values)
    {
        Values = values.ToList<IValue>();
    }

    public override ValueType GetType() => ValueType.Array;

    public override string ToString()
    {
        return Values.Count > 0
            ? "{" + string.Join(", ", Values.Select(v => v.Value)) + "}"
            : "{|}";
    }

    public IPatternNode GetRoot()
    {
        var patterns = new List<IPatternNode>();

        foreach (var element in Values)
        {
            if (element.Value is not IPattern pattern)
                throw new Throw("The elements of an array pattern must be patterns");

            patterns.Add(pattern.GetRoot());
        }

        return new ArrayPattern(patterns, null, -1);
    }

    public override void Destroy()
    {
        foreach (var variable in Values.Cast<ArrayVariable>())
            variable.Delete(true);
    }

    public override Value Copy(bool assign)
    {
        var array = new Array
        {
            _assigned = assign
        };

        array.Values = assign
            ? Values.Select(x => new ArrayVariable(x.Value.GetOrCopy(assign), array)).ToList<IValue>()
            : Values.Select(x => x.Value.GetOrCopy(assign)).ToList<IValue>();

        return array;
    }

    public override Value GetOrCopy(bool assign)
    {
        var array = _assigned
            ? new Array()
            : this;

        array._assigned = assign;
        array.Values = assign
            ? Values.Select(x => new ArrayVariable(x.Value.GetOrCopy(assign), array)).ToList<IValue>()
            : Values.Select(x => x.Value.GetOrCopy(assign)).ToList<IValue>();

        return array;
    }

    public IValue Index(Value value, Call call)
    {
        if (value is Number number)
        {
            int index = number.GetInt();

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
            [var value, Number number] => new(Enumerable.Repeat(value, number.GetInt()).ToList()),
            [_] => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [_, _] => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [..] => throw new Throw($"'array' does not have a constructor that takes {values.Count} arguments")
        };
    }
}