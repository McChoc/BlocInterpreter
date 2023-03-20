using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Members;

internal sealed record StaticlyNamedMember(string Name, IExpression Expression) : IMember
{
    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var value = Expression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (Name, value);
    }
}