using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Null : Value
    {
        public static Null Value { get; } = new();

        public override VariableType Type => VariableType.Null;

        private Null() { }

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            return other.Value is Null;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.False as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = String.Empty as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.False,
                VariableType.String => String.Empty,
                _ => new Throw($"Cannot implicitly cast null as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.False,
                VariableType.String => String.Empty,
                _ => new Throw($"Cannot cast null as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => "null";
    }
}
