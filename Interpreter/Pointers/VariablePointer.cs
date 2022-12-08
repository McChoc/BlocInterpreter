using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal sealed class VariablePointer : Pointer
    {
        internal Variable? Variable { get; private set; }

        internal VariablePointer(Variable? variable)
        {
            Variable = variable;
            variable?.Pointers.Add(this);
        }

        internal override Value Get()
        {
            if (Variable is null)
                throw new Throw("Invalid reference");

            return Variable.Value;
        }

        internal override Value Set(Value value)
        {
            if (Variable is null)
                throw new Throw("Invalid reference");

            var old = Variable.Value;
            Variable.Value = value.Copy();
            old.Destroy();

            return Variable.Value;
        }

        internal override Value Delete()
        {
            if (Variable is null)
                throw new Throw("Invalid reference");

            var value = Variable.Value.Copy();
            Variable.Delete();
            return value;
        }

        internal override void Invalidate()
        {
            Variable = null;
        }

        internal override bool Equals(Pointer other)
        {
            return other is VariablePointer pointer &&
                Variable is not null &&
                pointer.Variable is not null &&
                Variable == pointer.Variable;
        }
    }
}