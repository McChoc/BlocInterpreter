using System;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers;

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

        value = value.GetOrCopy(true);
        Variable.Value.Destroy();
        return Variable.Value = value;
    }

    internal override Value Delete()
    {
        if (Variable is null)
            throw new Throw("Invalid reference");

        var value = Variable.Value.GetOrCopy();
        Variable.Delete();
        return value;
    }

    internal override void Invalidate()
    {
        Variable = null;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Variable);
    }

    public override bool Equals(object other)
    {
        return other is VariablePointer pointer &&
            Variable is not null &&
            pointer.Variable is not null &&
            Variable == pointer.Variable;
    }
}