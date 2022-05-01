using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Globalization;

namespace CmmInterpretor.Values
{
    public class Number : Value
    {
        public static Number NaN { get; } = new(double.NaN);
        public static Number Infinity { get; } = new(double.PositiveInfinity);

        public double Value { get; }

        public override VariableType Type => VariableType.Number;

        public Number(double value) => Value = value;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is Number num)
                return Value == num.Value;

            return false;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = new Bool(Value != 0 && Value != double.NaN) as T;
                return true;
            }

            if (typeof(T) == typeof(Number))
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
                VariableType.Bool => new Bool(Value is not (0 or double.NaN)),
                VariableType.Number => this,
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot implicitly cast number as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => new Bool(Value is not (0 or double.NaN)),
                VariableType.Number => this,
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot cast number as {type.ToString().ToLower()}")
            };
        }

        public int ToInt() => (int)Value;

        public override string ToString(int _)
        {
            if (double.IsPositiveInfinity(Value))
                return "infinity";
            else if (double.IsNegativeInfinity(Value))
                return "-infinity";
            else
                return Value.ToString(CultureInfo.InvariantCulture).ToLower();
        }
    }
}
