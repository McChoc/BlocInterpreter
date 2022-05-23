using CmmInterpretor.Expressions;
using CmmInterpretor.Memory;

namespace CmmInterpretor.Values
{
    public abstract class Value : IValue, IExpression
    {
        Value IValue.Value => this;
        IValue IExpression.Evaluate(Call _) => this;

        public abstract ValueType Type { get; }

        public abstract Value Copy();
        public virtual void Assign() { }
        public virtual void Destroy() { }
        public virtual void Remove(object accessor) { }

        public abstract bool Equals(IValue other);

        public abstract T Implicit<T>() where T : Value;
        public abstract IValue Explicit(ValueType type);

        public override string ToString() => ToString(0);
        public abstract string ToString(int depth);

        public bool Is<T>(out T? value) where T : Value
        {
            try
            {
                value = Implicit<T>();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
