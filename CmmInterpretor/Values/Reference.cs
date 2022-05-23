using CmmInterpretor.Results;
using CmmInterpretor.Variables;

namespace CmmInterpretor.Values
{
    public class Reference : Value
    {
        public Variable? Variable { get; private set; }

        public override ValueType Type => ValueType.Reference;

        public Reference(Variable? variable) => Variable = variable;

        public override Value Copy()
        {
            var reference = new Reference(Variable);
            Variable?.References?.Add(reference);
            return reference;
        }

        public void Invalidate()
        {
            Variable = null;
        }

        public override bool Equals(IValue other)
        {
            if (other.Value is not Reference r)
                return false;

            if (Variable != r.Variable)
                return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Reference))
                return (this as T)!;

            if (Variable is not null)
                return Variable.Value.Implicit<T>();

            throw new Throw("Invalid reference");
        }

        public override IValue Explicit(ValueType type)
        {
            if (type == ValueType.Reference)
                return this;

            if (Variable is not null)
                return Variable.Value.Explicit(type);

            throw new Throw("Invalid reference");
        }

        public override string ToString(int _) => "[reference]";
    }
}
