using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.StructMembers;

internal sealed record StaticMember : IMember
{
    private readonly string _name;
    private readonly IExpression _expression;

    public StaticMember(string name, IExpression expression)
    {
        _name = name;
        _expression = expression;
    }

    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var value = _expression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (_name, value);
    }
}