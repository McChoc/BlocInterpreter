using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Comparers;
using Bloc.Utils.Extensions;
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
            ? Values.Select(x => new ArrayVariable(x.Value.Copy(assign), array)).ToList<IValue>()
            : Values.Select(x => x.Value.Copy(assign)).ToList<IValue>();

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

    public IValue Index(List<Value> args, Call call)
    {
        return args switch
        {
            [] => throw new Throw($"The array indexer does not take 0 arguments"),
            [Range { Step: null } range] when _assigned => new ContiguousSlicePointer(this, range),
            [Number number, ..] => IndexJaggedArray(IndexByNumber(number), args.GetRange(1..), call),
            [Range range, ..] => IndexJaggedArray(IndexByRange(range), args.GetRange(1..), call),
            [Tuple tuple, ..] => IndexJaggedArray(IndexByTuple(tuple), args.GetRange(1..), call),
            _ => throw new Throw($"The array indexer does not takes a '{args[0].GetTypeName()}'"),
        };
    }

    private IValue IndexJaggedArray(IValue value, List<Value> args, Call call)
    {
        if (args.Count == 0)
            return value is ArrayVariable variable
                ? new VariablePointer(variable)
                : value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit);

        if (value.Value is not IIndexable indexable)
            throw new Throw("The '[]' operator can only be apllied to strings and arrays");

        return indexable.Index(args, call);
    }

    private IValue IndexJaggedArray(IEnumerable<IValue> values, List<Value> args, Call call)
    {
        var indexedValues = values
            .Select(x => IndexJaggedArray(x, args, call))
            .ToList();

        if (_assigned)
        {
            var pointers = indexedValues
                .Cast<Pointer>()
                .ToList();

            return new SlicePointer(pointers);
        }
        else
        {
            var elements = indexedValues
                .Select(x => x.Value)
                .ToList();

            return new Array(elements);
        }
    }

    private IValue IndexByNumber(Number number)
    {
        int index = number.GetInt();

        if (index < 0)
            index += Values.Count;

        if (index < 0 || index >= Values.Count)
            throw new Throw("Index out of range");

        return Values[index];
    }

    private IEnumerable<IValue> IndexByRange(Range range)
    {
        var (start, end, step) = RangeHelper.Deconstruct(range, Values.Count);

        for (int i = start; i != end && i < end == step > 0; i += step)
            yield return Values[i];
    }

    private IEnumerable<IValue> IndexByTuple(Tuple tuple)
    {
        foreach (var index in tuple.Values)
        {
            if (index.Value is Number number)
            {
                yield return IndexByNumber(number);
                continue;
            }

            var values = index.Value switch
            {
                Range range => IndexByRange(range),
                Tuple nested => IndexByTuple(nested),
                _ => throw new Throw($"The array indexer does not takes a '{index.Value.GetTypeName()}'")
            };

            foreach (var value in values)
                yield return value;
        }
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
            [var value] => throw new Throw($"'array' does not have a constructor that takes a '{value.GetTypeName()}'"),
            [_, _] => throw new Throw($"'array' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [..] => throw new Throw($"'array' does not have a constructor that takes {values.Count} arguments")
        };
    }
}