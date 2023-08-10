using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Identifiers;

internal sealed record NameIdentifier : IIdentifier
{
    private readonly string _name;

    public NameIdentifier(string name)
    {
        _name = name;
    }

    public IValue Define(Value value, Call call, bool mask = false, bool mutable = true)
    {
        return call.Set(mask, mutable, _name, value.GetOrCopy(true));
    }
}