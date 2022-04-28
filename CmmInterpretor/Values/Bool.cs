using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Bool : Value
    {
        public static Bool False => new(false);
        public static Bool True => new(true);

        public bool Value { get; set; }

        public Bool() => Value = false;
        public Bool(bool value) => Value = value;

        public override VariableType TypeOf() => VariableType.Bool;

        public override Value Copy() => new Bool(Value);

        public override bool Equals(Value other)
        {
            if (other is Bool b)
                return Value == b.Value;

            return false;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = this as T;
                return true;
            }

            if (typeof(T) == typeof(Number))
            {
                value = Value ? new Number(1) as T : new Number(0) as T;
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
                return this;

            if (typeof(T) == typeof(Number))
                return Value ? new Number(1) : new Number(0);

            if (typeof(T) == typeof(String))
                return new String(ToString());

            return new Throw($"Cannot cast bool as {typeof(T)}");
        }

        public override string ToString(int _) => Value ? "true" : "false";
    }
}
