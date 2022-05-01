using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Bool : Value
    {
        public static Bool False { get; } = new(false);
        public static Bool True { get; } = new(true);

        public bool Value { get; }

        public override VariableType Type => VariableType.Bool;

        public Bool(bool value) => Value = value;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is Bool b)
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

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => this,
                VariableType.Number => new Number(Value ? 1 : 0),
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot implicitly cast bool as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => this,
                VariableType.Number => new Number(Value ? 1 : 0),
                VariableType.String => new String(ToString()),
                _ => new Throw($"Cannot cast bool as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => Value ? "true" : "false";
    }
}
