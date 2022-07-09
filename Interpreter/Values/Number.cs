﻿using System.Globalization;
using Bloc.Results;

namespace Bloc.Values
{
    public class Number : Value
    {
        public Number(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public override ValueType GetType() => ValueType.Number;

        public override bool Equals(IValue other)
        {
            if (other.Value is Number num)
                return Value == num.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (new Bool(Value is not (0 or double.NaN)) as T)!;

            if (typeof(T) == typeof(Number))
                return (this as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            throw new Throw($"Cannot implicitly cast number as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => new Bool(Value is not (0 or double.NaN)),
                ValueType.Number => this,
                ValueType.String => new String(ToString()),
                _ => throw new Throw($"Cannot cast number as {type.ToString().ToLower()}")
            };
        }

        public int ToInt()
        {
            return (int)Value;
        }

        public override string ToString(int _)
        {
            if (double.IsPositiveInfinity(Value))
                return "infinity";

            if (double.IsNegativeInfinity(Value))
                return "-infinity";

            return Value.ToString(CultureInfo.InvariantCulture).ToLower();
        }
    }
}