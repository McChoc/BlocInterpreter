using System.Collections.Generic;
using Bloc.Results;

namespace Bloc.Values
{
    public sealed class Range : Value
    {
        public Range(int? start, int? end, int? step)
        {
            Start = start;
            End = end;
            Step = step ?? 1;
        }

        public int? Start { get; }
        public int? End { get; }
        public int Step { get; }

        internal override ValueType GetType() => ValueType.Range;

        public override bool Equals(Value other)
        {
            if (other is not Range range)
                return false;

            if (Start != range.Start)
                return false;

            if (End != range.End)
                return false;

            if (Step != range.Step)
                return false;

            return true;
        }

        internal static Range Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(null, null, null),
                1 => values[0] switch
                {
                    Null => new(null, null, null),
                    Number end => new(null, end.GetInt(), null),
                    Range range => range,
                    var value => throw new Throw($"'range' does not have a constructor that takes a '{value.GetType().ToString().ToLower()}'")
                },
                2 => (values[0], values[1]) switch
                {
                    (Null, Null) => new(null, null, null),
                    (Number start, Null) => new(start.GetInt(), null, null),
                    (Null, Number end) => new(null, end.GetInt(), null),
                    (Number start, Number end) => new(start.GetInt(), end.GetInt(), null),
                    var value => throw new Throw($"'range' does not have a constructor that takes a '{value.Item1.GetType().ToString().ToLower()}' and a '{value.Item2.GetType().ToString().ToLower()}'")
                },
                3 => (values[0], values[1], values[2]) switch
                {
                    (Null, Null, Null) => new(null, null, null),
                    (Number start, Null, Null) => new(start.GetInt(), null, null),
                    (Null, Number end, Null) => new(null, end.GetInt(), null),
                    (Number start, Number end, Null) => new(start.GetInt(), end.GetInt(), null),
                    (Null, Null, Number step) => new(null, null, step.GetInt()),
                    (Number start, Null, Number step) => new(start.GetInt(), null, step.GetInt()),
                    (Null, Number end, Number step) => new(null, end.GetInt(), step.GetInt()),
                    (Number start, Number end, Number step) => new(start.GetInt(), end.GetInt(), step.GetInt()),
                    var value => throw new Throw($"'range' does not have a constructor that takes a '{value.Item1.GetType().ToString().ToLower()}', a '{value.Item2.GetType().ToString().ToLower()}' and a '{value.Item3.GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'range' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString(int _)
        {
            return $"{Start}..{End}{(Step != 1 ? $"..{Step}" : "")}";
        }
    }
}