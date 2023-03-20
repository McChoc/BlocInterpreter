using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.SubExpressions;

internal sealed record DynamiclyNamedMember(IExpression NameExpression, IExpression ValueExpression) : IMember
{
    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var nameValue = NameExpression.Evaluate(call);
        var nameString = String.ImplicitCast(nameValue);
        var name = nameString.Value;

        var value = ValueExpression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (name, value);
    }
}
