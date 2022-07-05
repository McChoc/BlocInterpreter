namespace Bloc.Values
{
    public interface IValue
    {
        public Value Value { get; }
        public ValueType Type { get; }

        public Value Copy();
        public void Assign();
        public void Destroy();

        public bool Equals(IValue other);

        public bool Is<T>(out T? value) where T : Value;
        public T Implicit<T>() where T : Value;
        public IValue Explicit(ValueType type);

        public string ToString(int depth);
    }
}