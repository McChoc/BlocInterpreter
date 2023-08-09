using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.StructMembers;

internal sealed record StaticMember(string Name, IExpression Expression) : IMember
{
    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var value = Expression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (Name, value);
    }
}