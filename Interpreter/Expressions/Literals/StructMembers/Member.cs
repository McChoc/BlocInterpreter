using System.Collections.Generic;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.StructMembers;

internal sealed record Member : IMember
{
    private readonly INamedIdentifier _identifier;
    private readonly IExpression _expression;

    public Member(INamedIdentifier identifier, IExpression expression)
    {
        _identifier = identifier;
        _expression = expression;
    }

    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var name = _identifier.GetName(call);
        var value = _expression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (name, value);
    }
}