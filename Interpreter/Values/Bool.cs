using Bloc.Results;

namespace Bloc.Values
{
    public class Bool : Value
    {
        internal Bool(bool value)
        {
            Value = value;
        }

        public static Bool False { get; } = new(false);
        public static Bool True { get; } = new(true);

        public bool Value { get; }

        public override ValueType Type => ValueType.Bool;

        public override Value Copy()
        {
            return this;
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is Bool b)
                return Value == b.Value;

            return false;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (this as T)!;

            if (typeof(T) == typeof(Number))
                return (new Number(Value ? 1 : 0) as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            throw new Throw($"Cannot implicitly cast bool as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => this,
                ValueType.Number => new Number(Value ? 1 : 0),
                ValueType.String => new String(ToString()),
                _ => throw new Throw($"Cannot cast bool as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return Value ? "true" : "false";
        }
    }
}