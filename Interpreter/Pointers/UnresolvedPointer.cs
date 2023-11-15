using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Pointers;

public sealed class UnresolvedPointer : Pointer
{
    internal string Name { get; }

    internal Variable? Local { get; }
    internal Variable? Param { get; }
    internal Variable? Outer { get; }
    internal Variable? Toplvl { get; }
    internal Variable? Global { get; }

    internal UnresolvedPointer(string name, Variable? local, Variable? param, Variable? outer, Variable? toplvl, Variable? global)
    {
        Name = name;

        Local = local;
        Param = param;
        Outer = outer;
        Toplvl = toplvl;
        Global = global;
    }

    public override Value Get() => Resolve().Get();
    public override Value Set(Value value) => Resolve().Set(value);
    public override Value Delete() => Resolve().Delete();

    internal VariablePointer Resolve()
    {
        var variable = Local ?? Param ?? Outer ?? Toplvl ?? Global ?? throw new Throw($"Variable {Name} was not defined in scope");

        return new VariablePointer(variable);
    }
}