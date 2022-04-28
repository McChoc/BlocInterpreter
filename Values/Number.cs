using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System.Globalization;

namespace CmmInterpretor.Values
{
    public class Number : Value
    {
        public static Number NaN => new(double.NaN);
        public static Number Infinity => new(double.PositiveInfinity);

        public double Value { get; set; }

        public Number() => Value = 0;
        public Number(double value) => Value = value;

        public override VariableType TypeOf() => VariableType.Number;

        public override Value Copy() => new Number(Value);

        public override bool Equals(Value other)
        {
            if (other is Number num)
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return new Bool(Value != 0 && Value != double.NaN);

            if (typeof(T) == typeof(Number))
                return this;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            return new Throw($"Cannot cast number as {typeof(T)}");
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
