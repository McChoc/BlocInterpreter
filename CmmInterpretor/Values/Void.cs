using CmmInterpretor.Data;
using CmmInterpretor.Results;
using System;

namespace CmmInterpretor.Values
{
    public class Void : Value
    {
        public override VariableType TypeOf() => VariableType.Void;

        public override Value Copy() => throw new Exception();

        public override bool Equals(Value other) => throw new Exception();

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Type))
            {
                value = new Type(VariableType.Void) as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Type))
                return new Type(VariableType.Void);

            return new Throw($"Cannot cast void as {typeof(T)}");
        }

        public override string ToString(int _) => "void";
    }
}
