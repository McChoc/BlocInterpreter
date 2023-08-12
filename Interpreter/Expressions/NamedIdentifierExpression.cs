using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions;

internal sealed record NamedIdentifierExpression : IExpression
{
    internal INamedIdentifier Identifier { get; }

    internal NamedIdentifierExpression(INamedIdentifier identifier)
    {
        Identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        var name = Identifier.GetName(call);

        return call.Get(name);
    }
}