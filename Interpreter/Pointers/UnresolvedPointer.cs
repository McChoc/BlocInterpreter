using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers;

internal sealed class UnresolvedPointer : Pointer
{
    internal string Name { get; }

    internal Variable? Local { get; }
    internal Variable? Param { get; }
    internal Variable? NonLocal { get; }
    internal Variable? Global { get; }

    internal UnresolvedPointer(string name, Variable? local, Variable? param, Variable? nonLocal, Variable? global)
    {
        Name = name;

        Local = local;
        Param = param;
        NonLocal = nonLocal;
        Global = global;
    }

    internal override Value Get() => Resolve().Get();
    internal override Value Set(Value value) => Resolve().Set(value);
    internal override Value Delete() => Resolve().Delete();

    internal VariablePointer Resolve()
    {
        var variable = Local ?? Param ?? NonLocal ?? Global ?? throw new Throw($"Variable {Name} was not defined in scope");

        return new VariablePointer(variable);
    }
}