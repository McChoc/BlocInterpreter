using CmmInterpretor.Results;

namespace CmmInterpretor.Data
{
    public abstract class Value : IValue, IResult
    {
        Value IValue.Value => this;
        public abstract VariableType Type { get; }

        public abstract Value Copy();
        public virtual void Assign() { }
        public virtual void Destroy() { }
        public virtual void Remove(object accessor) { }

        public abstract bool Equals(IValue other);

        public abstract bool Implicit<T>(out T value) where T : Value;
        public abstract IResult Implicit(VariableType type);
        public abstract IResult Explicit(VariableType type);

        public override string ToString() => ToString(0);
        public abstract string ToString(int depth);
    }
}
