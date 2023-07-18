using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Identifiers;

internal sealed class NameIdentifier : IIdentifier
{
    internal string Name { get; }

    public NameIdentifier(string name)
    {
        Name = name;
    }

    public IValue Define(Value value, Call call, bool mask = false, bool mutable = true)
    {
        return call.Set(mask, mutable, Name, value.GetOrCopy(true));
    }
}