using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Void : Value
    {
        public static Void Value { get; } = new();

        public override VariableType Type => VariableType.Void;

        private Void() { }

        public override Value Copy() => this;

        public override bool Equals(IValue other) => throw new System.Exception();

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(TypeCollection))
            {
                value = new TypeCollection(VariableType.Void) as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            if (type == VariableType.Void)
                return new TypeCollection(VariableType.Void);

            return new Throw($"Cannot implicitly cast void as {type.ToString().ToLower()}");
        }

        public override IResult Explicit(VariableType type)
        {
            if (type == VariableType.Void)
                return new TypeCollection(VariableType.Void);

            return new Throw($"Cannot cast void as {type.ToString().ToLower()}");
        }

        public override string ToString(int _) => "void";
    }
}
