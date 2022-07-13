using System.Globalization;
using Bloc.Results;

namespace Bloc.Values
{
    public class Number : Value
    {
        public Number() => Value = 0;

        public Number(double value) => Value = value;

        public double Value { get; }

        public override ValueType GetType() => ValueType.Number;

        public override bool Equals(Value other)
        {
            if (other is Number number)
                return Value == number.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (new Bool(Value is not (0 or double.NaN)) as T)!;

            if (typeof(T) == typeof(Number))
                return (this as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            throw new Throw($"Cannot implicitly cast number as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => new Bool(Value is not (0 or double.NaN)),
                ValueType.Number => this,
                ValueType.String => new String(ToString()),
                _ => throw new Throw($"Cannot cast number as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            if (double.IsNaN(Value))
                return "nan";

            if (double.IsPositiveInfinity(Value))
                return "infinity";

            if (double.IsNegativeInfinity(Value))
                return "-infinity";

            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public int ToInt()
        {
            if (double.IsNaN(Value))
                return 0;

            if (Value > int.MaxValue)
                return int.MaxValue;

            if (Value < int.MinValue)
                return int.MinValue;

            return (int)Value;
        }
    }
}