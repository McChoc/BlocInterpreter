using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Void : Value
    {
        public static Void Value { get; } = new();

        public override ValueType Type => ValueType.Void;

        private Void() { }

        public override Value Copy() => this;

        public override void Assign() => throw new Throw("You cannot assign void to a variable");

        public override bool Equals(IValue other)
        {
            return other.Value is Void;
        }

        public override T Implicit<T>()
        {
            throw new Throw($"Cannot implicitly cast void as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            throw new Throw($"Cannot cast void as {type.ToString().ToLower()}");
        }

        public override string ToString(int _) => "void";
    }
}
