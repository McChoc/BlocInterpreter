using CmmInterpretor.Results;

namespace CmmInterpretor.Data
{
    public interface IValue
    {
        public Value Value { get; }
        public VariableType Type { get; }

        public Value Copy();
        public void Assign();
        public void Destroy();

        public bool Equals(IValue other);

        public bool Implicit<T>(out T value) where T : Value;
        public IResult Implicit(VariableType type);
        public IResult Explicit(VariableType type);

        public string ToString(int depth);
    }
}
