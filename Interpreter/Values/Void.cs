using Bloc.Results;

namespace Bloc.Values
{
    public class Void : Value
    {
        private Void() { }

        public static Void Value { get; } = new();

        public override ValueType GetType() => ValueType.Void;

        internal override void Assign()
        {
            throw new Throw("You cannot assign void to a variable");
        }

        public override bool Equals(Value other)
        {
            return other is Void;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Void))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast void as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            if (type == ValueType.Void)
                return this;

            throw new Throw($"Cannot cast void as {type.ToString().ToLower()}");
        }

        public override string ToString(int _)
        {
            return "void";
        }
    }
}