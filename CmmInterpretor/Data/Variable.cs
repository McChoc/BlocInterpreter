using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public abstract class Variable : IValue, IResult
    {
        public Value Value { get; set; }
        public VariableType Type => Value.Type;

        public List<Reference> References { get; } = new();

        protected Variable(Value value) => Value = value;

        Value IValue.Copy() => Value.Copy();
        void IValue.Assign() => Value.Assign();
        public abstract void Destroy();

        bool IValue.Equals(IValue other) => Value.Equals(other);

        bool IValue.Implicit<T>(out T value) => Value.Implicit(out value);
        IResult IValue.Implicit(VariableType type) => Value.Implicit(type);
        IResult IValue.Explicit(VariableType type) => Value.Explicit(type);

        string IValue.ToString(int depth) => Value.ToString(depth);
    }
}
