using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Results;

namespace Bloc.Values
{
    public class Range : Value, IIterable
    {
        public Range() : this(null, null) { }

        public Range(int? start, int? end, int step = 1)
        {
            Start = start;
            End = end;
            Step = step;
        }

        public int? Start { get; }
        public int? End { get; }
        public int Step { get; }

        public override ValueType GetType() => ValueType.Range;

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

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(Range))
                return (this as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            throw new Throw($"Cannot implicitly cast range as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.Range => this,
                ValueType.String => new String(ToString()),
                _ => throw new Throw($"Cannot cast range as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return $"{Start}..{End}{(Step != 1 ? $"..{Step}" : "")}";
        }

        public IEnumerable<Value> Iterate()
        {
            double start = Start ?? (Step >= 0 ? 0 : -1);
            double end = End ?? (Step >= 0 ? double.PositiveInfinity : double.NegativeInfinity);

            for (var i = start; i * Step < end * Step; i += Step)
                yield return new Number(i);
        }
    }
}