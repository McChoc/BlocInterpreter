using System.Collections.Generic;
using System.Linq;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Comparers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Tuple : Value, IPattern
{
    [DoNotCompare]
    private bool _assigned = false;

    [DoNotCompare]
    public bool Assignable { get; private set; }

    [CompareUsing<ValueEqualityComparer>]
    public List<IValue> Values { get; private set; }

    public Tuple()
    {
        Assignable = false;
        Values = new();
    }

    public Tuple(List<IValue> values)
    {
        Assignable = true;
        Values = values;
    }

    public Tuple(List<Value> values)
    {
        Assignable = false;
        Values = values.ToList<IValue>();
    }

    public override ValueType GetType() => ValueType.Tuple;

    public override string ToString()
    {
        return Values.Count switch
        {
            0 => "()",
            1 => $"({Values[0].Value},)",
            _ => $"({string.Join(", ", Values.Select(v => v.Value))})"
        };
    }

    public IPatternNode GetRoot()
    {
        var patterns = new List<IPatternNode>();

        foreach (var element in Values)
        {
            if (element.Value is not IPattern pattern)
                throw new Throw("The elements of a tuple pattern must be patterns");

            patterns.Add(pattern.GetRoot());
        }

        return new TuplePattern(patterns);
    }

    public override Value Copy(bool assign)
    {
        return new Tuple
        {
            _assigned = assign,
            Assignable = false,
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
        tuple.Assignable = false;
        tuple.Values = Values
            .Select(x => x.Value.GetOrCopy(assign))
            .ToList<IValue>();

        return tuple;
    }

    private Tuple AsNonAssignable()
    {
        Assignable = false;
        return this;
    }

    internal static Tuple Construct(List<Value> values)
    {
        return values switch
        {
            [] or [Null] => new(),

            [Tuple tuple] => tuple.AsNonAssignable(),

            [Array array] => new(array.Values
                .Select(x => x.Value)
                .ToList()),

            [Struct @struct] => new(@struct.Values
                .OrderBy(x => x.Key)
                .Select(x => x.Value.Value)
                .ToList()),

            [Range range] => new(new List<Value>()
            {
                range.Start is int start ? new Number(start) : Null.Value,
                range.End is int end ? new Number(end) : Null.Value,
                range.Step is int step ? new Number(step) : Null.Value
            }),

            [_] => throw new Throw($"'tuple' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [..] => throw new Throw($"'tuple' does not have a constructor that takes {values.Count} arguments")
        };
    }
}