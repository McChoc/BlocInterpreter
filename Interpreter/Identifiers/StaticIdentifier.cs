using Bloc.Memory;
using Bloc.Values.Core;

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

    public IValue Define(Value value, Call call, bool mask = false, bool mutable = true)
    {
        return call.Set(mask, mutable, _name, value.GetOrCopy(true));
    }
}