using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Complex : Value
    {
        public Complex(object? value)
        {
            Value = value;
        }

        public object? Value { get; }

        public override ValueType Type => ValueType.Complex;

        public override Value Copy()
        {
            return this;
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is Complex c)
                return Value == c.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Complex))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast complex as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Complex => this,
                _ => throw new Throw($"Cannot cast complex as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return "[complex]";
        }
    }
}