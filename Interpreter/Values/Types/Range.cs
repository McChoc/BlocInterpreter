using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Constants;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Range : Value
{
    public Index Start { get; }
    public Index Stop { get; }
    public double? Step { get; }

    public record Index(double? Value, bool Inclusive);

    public Range()
    {
        Start = new Index(null, true);
        Stop = new Index(null, true);
    }

    public Range(Index start, Index stop, double? step)
    {
        Start = start;
        Stop = stop;
        Step = step;
    }

    public override ValueType GetType() => ValueType.Range;

    public override string ToString()
    {
        string? start = Start.Value?.ToString(CultureInfo.InvariantCulture).ToLower();
        string? stop = Stop.Value?.ToString(CultureInfo.InvariantCulture).ToLower();
        string? step = Step?.ToString(CultureInfo.InvariantCulture);

        var symbol = this switch
        {
            { Start.Inclusive: true, Stop.Inclusive: true } => Symbol.RANGE_INC_INC,
            { Start.Inclusive: true, Stop.Inclusive: false } => Symbol.RANGE_INC_EXC,
            { Start.Inclusive: false, Stop.Inclusive: true } => Symbol.RANGE_EXC_INC,
            { Start.Inclusive: false, Stop.Inclusive: false } => Symbol.RANGE_EXC_EXC,
        };

        return Step is null
            ? $"{start}{symbol}{stop}"
            : $"{start}{symbol}{stop}:{step}";
    }

    internal static Range Construct(List<Value> values)
    {
        return values switch
        {
            [] => new(),

            [Range range] => range,

            [Null or INumeric] => new Range(
                new Index(null, true),
                new Index(values[0] is INumeric stop ? stop.GetDouble() : null, true),
                null),

            [Null or INumeric, Null or INumeric] => new Range(
                new Index(values[0] is INumeric start ? start.GetDouble() : null, true),
                new Index(values[1] is INumeric stop ? stop.GetDouble() : null, true),
                null),

            [Null or INumeric, Null or INumeric, Null or INumeric] => new Range(
                new Index(values[0] is INumeric start ? start.GetDouble() : null, true),
                new Index(values[1] is INumeric stop ? stop.GetDouble() : null, true),
                values[2] is INumeric step ? step.GetDouble() : null),

            [Null or INumeric, Null or INumeric, Null or INumeric, Bool includeStart] => new Range(
                new Index(values[0] is INumeric start ? start.GetDouble() : null, includeStart.Value),
                new Index(values[1] is INumeric stop ? stop.GetDouble() : null, true),
                values[2] is INumeric step ? step.GetDouble() : null),

            [Null or INumeric, Null or INumeric, Null or INumeric, Bool includeStart, Bool includeStop] => new Range(
                new Index(values[0] is INumeric start ? start.GetDouble() : null, includeStart.Value),
                new Index(values[1] is INumeric stop ? stop.GetDouble() : null, includeStop.Value),
                values[2] is INumeric step ? step.GetDouble() : null),

            [_] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}'"),
            [_, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}' and a '{values[1].GetTypeName()}'"),
            [_, _, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}', a '{values[1].GetTypeName()}' and a '{values[2].GetTypeName()}'"),
            [_, _, _, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}', a '{values[1].GetTypeName()}', a '{values[2].GetTypeName()}' and a '{values[3].GetTypeName()}'"),
            [_, _, _, _, _] => throw new Throw($"'range' does not have a constructor that takes a '{values[0].GetTypeName()}', a '{values[1].GetTypeName()}', a '{values[2].GetTypeName()}', a '{values[3].GetTypeName()}' and a '{values[4].GetTypeName()}'"),
            [..] => throw new Throw($"'range' does not have a constructor that takes {values.Count} arguments")
        };
    }
}