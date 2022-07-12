using Bloc.Results;

namespace Bloc.Values
{
    public class Complex : Value
    {
        public Complex() => Value = null;

        public Complex(object? value) => Value = value;

        public object? Value { get; }

        public override ValueType GetType() => ValueType.Complex;

        public override bool Equals(Value other)
        {
            if (other is Complex complex)
                return Value == complex.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Complex))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast complex as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
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