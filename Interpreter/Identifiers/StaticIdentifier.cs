using System.Xml.Linq;
using Bloc.Memory;
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

    public IValue Define(Value value, Call call, bool mask, bool mutable, VariableScope scope)
    {
        return call.Set(_name, value.GetOrCopy(true), mutable, mask, scope);
    }
}