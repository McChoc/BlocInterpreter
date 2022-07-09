﻿using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Results;

namespace Bloc.Values
{
    public class Range : Value, IIterable
    {
        public Range(int? start, int? end, int step = 1)
        {
            (Start, End, Step) = (start, end, step);
        }

        public int? Start { get; }
        public int? End { get; }
        public int Step { get; }

        public override ValueType GetType() => ValueType.Range;

        public IEnumerable<Value> Iterate()
        {
            double start = Start ?? (Step >= 0 ? 0 : -1);
            var end = End ?? (Step >= 0 ? double.PositiveInfinity : double.NegativeInfinity);

            if (Step > 0)
                for (var i = start; i < end; i += Step)
                    yield return new Number(i);

            if (Step < 0)
                for (var i = start; i > end; i += Step)
                    yield return new Number(i);
        }

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

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(Range))
                return (this as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            throw new Throw($"Cannot implicitly cast range as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
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
    }
}