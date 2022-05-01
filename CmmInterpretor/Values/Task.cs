using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Task : Value
    {
        public override VariableType Type => VariableType.Task;

        public override Value Copy() => this;

        public override bool Equals(IValue other)
        {
            if (other.Value is not Task task)
                return false;
            
            // TODO

            return true;
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

            if (typeof(T) == typeof(Task))
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
                VariableType.Task => this,
                _ => new Throw($"Cannot implicitly cast task as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Task => this,
                _ => new Throw($"Cannot cast task as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => "[task]";
    }
}
