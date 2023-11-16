using Bloc.Core;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Variables;

namespace Bloc.Identifiers;

internal sealed record StaticIdentifier : INamedIdentifier
{
    private readonly string _name;

    public StaticIdentifier(string name)
    {
        _name = name;
    }

    public string GetName(Call _)
    {
        return _name;
    }

    public Value From(Module module, Call _)
    {
        if (!module.Exports.TryGetValue(_name, out var export))
            throw new Throw($"Module '{module.Path}' does not export {_name}");

        return export.Copy();
    }

    public IValue Define(Value value, Call call, bool mask, bool mutable, VariableScope scope)
    {
        return call.Set(_name, value.GetOrCopy(true), mutable, mask, scope);
    }
}