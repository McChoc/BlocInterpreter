using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Task : Value
    {
        public override VariableType TypeOf() => VariableType.Task;

        public override Value Copy() => this;

        public override bool Equals(Value other)
        {
            if (other is Task task)
                return true; // TODO

            return false;
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.True;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            if (typeof(T) == typeof(Task))
                return this;

            return new Throw($"Cannot cast task as {typeof(T)}");
        }

        public override string ToString(int _) => "[task]";
    }
}
