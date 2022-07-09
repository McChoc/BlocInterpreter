using Bloc.Results;

namespace Bloc.Values
{
    public class Null : Value
    {
        private Null() { }

        public static Null Value { get; } = new();

        public override ValueType GetType() => ValueType.Null;

        public override bool Equals(IValue other)
        {
            return other.Value is Null;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.False as T)!;

            if (typeof(T) == typeof(String))
                return (String.Empty as T)!;

            throw new Throw($"Cannot implicitly cast null as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
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