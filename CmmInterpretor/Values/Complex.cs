using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Complex : Value
    {
        public object Value { get; }

        public override VariableType Type => VariableType.Complex;

        public Complex(object value) => Value = value;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is Complex c)
                return Value == c.Value;

            return false;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            if (typeof(T) == typeof(Complex))
            {
                value = this as T;
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
                VariableType.String => new String(ToString()),
                VariableType.Complex => this,
                _ => new Throw($"Cannot implicitly cast complex as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Complex => this,
                _ => new Throw($"Cannot cast complex as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => "[complex]";
    }
}
