using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Values
{
    public class Range : Value, IIterable
    {
        public int? Start { get; }
        public int? End { get; }
        public int Step { get; }

        public override VariableType Type => VariableType.Range;

        public Range(int? start, int? end, int step = 1) => (Start, End, Step) = (start, end, step);

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is not Range rng)
                return false;

            if (Start != rng.Start)
                return false;

            if (End != rng.End)
                return false;

            if (Step != rng.Step)
                return false;

            return true;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;
                return true;
            }

            if (typeof(T) == typeof(Range))
            {
                value = this as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.Range => this,
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot implicitly cast range as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.Range => this,
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot cast range as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => $"{Start}..{End}{(Step != 1 ? $"..{Step}" : "")}";

        public IEnumerable<Value> Iterate()
        {
            double start = Start ?? (Step >= 0 ? 0 : -1);
            double end = End ?? (Step >= 0 ? double.PositiveInfinity : double.NegativeInfinity);

            if (Step > 1)
                for (double i = start; i < end; i += Step)
                    yield return new Number(i);

            if (Step < 1)
                for (double i = start; i > end; i += Step)
                    yield return new Number(i);

            yield break;
        }
    }
}
