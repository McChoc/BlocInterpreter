using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System;

namespace CmmInterpretor.Values
{
    public class Range : Value, IIterable
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int Step { get; set; }

        public Range() { }
        public Range(int? start, int? end, int step = 1) => (Start, End, Step) = (start, end, step);

        public override VariableType TypeOf() => VariableType.Range;

        public override Value Copy() => new Range(Start, End, Step);

        public override bool Equals(Value other)
        {
            if (other is not Range rng)
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.True;

            if (typeof(T) == typeof(Range))
                return this;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            return new Throw($"Cannot cast range as {typeof(T)}");
        }

        public override string ToString(int _) => $"{Start}..{End}{(Step != 1 ? $"..{Step}" : "")}";

        public int Count
        {
            get
            {
                int start = Start ?? (Step >= 0 ? 0 : -1);
                int end = End ?? (Step >= 0 ? 0 : -1);

                if (Step != 0 && (start >= 0 == (End == null ? Step < 0 : End >= 0)))
                {
                    int count = (end - start + Step - Math.Sign(Step)) / Step;

                    return count >= 0 ? count : 0;
                }

                return 0;
            }
        }
        public Value this[int index]
        {
            get
            {
                int start = Start ?? (Step >= 0 ? 0 : -1);

                int number = start + Step * index;

                if (Step == 0 || (start >= 0 != (End == null ? Step < 0 : End >= 0)))
                    throw new Exception();

                return new Number(number);
            }
        }
    }
}
