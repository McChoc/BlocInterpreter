using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal sealed class VariablePointer : Pointer
    {
        internal VariablePointer(Variable? variable)
        {
            Variable = variable;
            variable?.Pointers.Add(this);
        }

        internal Variable? Variable { get; private set; }

        internal override Pointer Define(Value value, Call call)
        {
            if (Variable is not StackVariable stackVariable)
                throw new Throw("The left part of an assignement must be a variable");

            if (call.Scopes[^1].Variables.ContainsKey(stackVariable.Name))
                throw new Throw("Variable was already defined in scope");

            return call.Set(stackVariable.Name, value);
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

            value = value.Copy();
            value.Assign();

            Variable.Value.Destroy();

            return Variable.Value = value;
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
            if (other is not VariablePointer pointer)
                return false;

            if (Variable is null || pointer.Variable is null)
                return false;

            return Variable == pointer.Variable;
        }
    }
}