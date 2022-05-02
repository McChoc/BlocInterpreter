using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Reference : Value
    {
        public Variable Variable { get; private set; }

        public override VariableType Type => VariableType.Reference;

        public Reference(Variable var) => Variable = var;

        public override Value Copy()
        {
            var reference = new Reference(Variable);
            Variable.References.Add(reference);
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

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Reference))
            {
                value = this as T;
                return true;
            }
            else if (Variable.Value.Implicit(out T val))
            {
                value = val;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            if (type == VariableType.Reference)
                return this;

            return Variable.Value.Explicit(type);
        }

        public override IResult Explicit(VariableType type) => Implicit(type);

        public override string ToString(int _) => "[reference]";
    }
}
