using System.Collections.Generic;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Range : Value
{
    public int? Start { get; }
    public int? Stop { get; }
    public int? Step { get; }
    public bool Inclusive { get; }

    public Range(int? start, int? stop, int? step, bool inclusive)
    {
        Start = start;
        Stop = stop;
        Step = step;
        Inclusive = inclusive;
    }

    public override ValueType GetType() => ValueType.Range;

    public override string ToString()
    {
        return this switch
        {
            { Step: null, Inclusive: false } => $"{Start}:{Stop}",
            { Step: null, Inclusive: true } => $"{Start}:={Stop}",
            { Step: not null, Inclusive: false } => $"{Start}:{Stop}:{Step}",
            { Step: not null, Inclusive: true } => $"{Start}:={Stop}:{Step}"
        };
    }

    internal static Range Construct(List<Value> values)
    {
        return values switch
        {
            [] => new(null, null, null, false),

            [Range range] => range,

            [Null or Number] => new(
                null,
                values[0] is Number stop ? stop.GetInt() : null,
                null,
                false),

            [Null or Number, Null or Number] => new(
                values[0] is Number start ? start.GetInt() : null,
                values[1] is Number stop ? stop.GetInt() : null,
                null,
                false),

            [Null or Number, Null or Number, Null or Number] => new(
                values[0] is Number start ? start.GetInt() : null,
                values[1] is Number stop ? stop.GetInt() : null,
                values[2] is Number step ? step.GetInt() : null,
                false),

            [_] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [_, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [_, _, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}', a '{values[1].GetTypeName()}' and a '{values[2].GetTypeName()}'"),
            [..] => throw new Throw($"'range' does not have a constructor that takes {values.Count} arguments")
        };
    }
}