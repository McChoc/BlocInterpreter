using CmmInterpretor.Results;

namespace CmmInterpretor.Data
{
    public abstract class Value : IValue, IResult
    {
        Value IValue.Value() => this;

        public abstract VariableType TypeOf();

        public abstract Value Copy();

        public abstract bool Equals(Value other);

        public abstract bool Implicit<T>(out T value) where T : Value;
        public abstract IResult Explicit<T>() where T : Value;

        public override string ToString() => ToString(0);
        public abstract string ToString(int depth);
    }
}
