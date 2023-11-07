using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Pointers;

public sealed class UnresolvedPointer : Pointer
{
    internal string Name { get; }

    internal Variable? Local { get; }
    internal Variable? Params { get; }
    internal Variable? Closure { get; }
    internal Variable? Module { get; }
    internal Variable? Global { get; }

    internal UnresolvedPointer(string name, Variable? local, Variable? @params, Variable? closure, Variable? module, Variable? global)
    {
        Name = name;

        Local = local;
        Params = @params;
        Closure = closure;
        Module = module;
        Global = global;
    }

    public override Value Get() => Resolve().Get();
    public override Value Set(Value value) => Resolve().Set(value);
    public override Value Delete() => Resolve().Delete();

    internal VariablePointer Resolve()
    {
        var variable = Local ?? Params ?? Closure ?? Module ?? Global ?? throw new Throw($"Variable {Name} was not defined in scope");

        return new VariablePointer(variable);
    }
}