namespace Bloc.Values
{
    public abstract class Value : IValue
    {
        Value IValue.Value => this;

        public abstract new ValueType GetType();

        public virtual Value Copy() => this;
        public virtual void Assign() { }
        public virtual void Destroy() { }
        public virtual void Remove(object accessor) { }

        public abstract bool Equals(IValue other);

        public abstract T Implicit<T>() where T : Value;
        public abstract IValue Explicit(ValueType type);
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

        public abstract string ToString(int depth);
        public override string ToString() => ToString(0);
    }
}