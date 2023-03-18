using System;
using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values;

public sealed class Range : Value
{
    public int? Start { get; }
    public int? End { get; }
    public int? Step { get; }

    public Range(int? start, int? end, int? step)
    {
        Start = start;
        End = end;
        Step = step;
    }

    internal override ValueType GetType() => ValueType.Range;

    internal static Range Construct(List<Value> values)
    {
        return values switch
        {
            [] => new(null, null, null),

            [Range range] => range,

            [Null or Number] => new(
                null,
                values[0] is Number end ? end.GetInt() : null,
                null),

            [Null or Number, Null or Number] => new(
                values[0] is Number start ? start.GetInt() : null,
                values[1] is Number end ? end.GetInt() : null,
                null),

            [Null or Number, Null or Number, Null or Number] => new(
                values[0] is Number start ? start.GetInt() : null,
                values[1] is Number end ? end.GetInt() : null,
                values[2] is Number step ? step.GetInt() : null),

            [_] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [_, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [_, _, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}', a '{values[1].GetTypeName()}' and a '{values[2].GetTypeName()}'"),
            [..] => throw new Throw($"'range' does not have a constructor that takes {values.Count} arguments")
        };
    }

    public override string ToString()
    {
        return Step is null
            ? $"{Start}:{End}"
            : $"{Start}:{End}:{Step}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End, Step);
    }

    public override bool Equals(object other)
    {
        return other is Range range &&
            Start == range.Start &&
            End == range.End &&
            Step == range.Step;
    }
}