using Bloc.Results;

namespace Bloc.Values
{
    public class Null : Value
    {
        private Null() { }

        public static Null Value { get; } = new();

        public override ValueType GetType() => ValueType.Null;

        public override bool Equals(Value other)
        {
            return other is Null;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (this as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.False as T)!;

            if (typeof(T) == typeof(String))
                return (String.Empty as T)!;

            throw new Throw($"Cannot implicitly cast null as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => this,
                ValueType.Bool => Bool.False,
                ValueType.String => String.Empty,
                _ => throw new Throw($"Cannot cast null as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return "null";
        }
    }
}