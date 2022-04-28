using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Null : Value
    {
        public override VariableType TypeOf() => VariableType.Null;

        public override Value Copy() => new Null();

        public override bool Equals(Value other)
        {
            return other is Null;
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
                value = new String() as T;
                return true;
            }

            if (typeof(T) == typeof(Array))
            {
                value = new Array() as T;
                return true;
            }

            if (typeof(T) == typeof(Struct))
            {
                value = new Struct() as T;
                return true;
            }

            if (typeof(T) == typeof(Type))
            {
                value = new Type(VariableType.Null) as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.False;

            if (typeof(T) == typeof(String))
                return new String();

            if (typeof(T) == typeof(Array))
                return new Array();

            if (typeof(T) == typeof(Struct))
                return new Struct();

            if (typeof(T) == typeof(Type))
                return new Type(VariableType.Null);

            return new Throw($"Cannot cast null as {typeof(T)}");
        }

        public override string ToString(int _) => "null";
    }
}
