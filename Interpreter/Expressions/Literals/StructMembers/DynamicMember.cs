using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.StructMembers;

internal sealed record DynamicMember : IMember
{
    private readonly IExpression _nameExpression;
    private readonly IExpression _valueExpression;

    public DynamicMember(IExpression nameExpression, IExpression valueExpression)
    {
        _nameExpression = nameExpression;
        _valueExpression = valueExpression;
    }

    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var nameValue = _nameExpression.Evaluate(call);
        var nameString = String.ImplicitCast(nameValue);
        string name = nameString.Value;

        if (!IdentifierHelper.IsIdentifierValid(name))
            throw new Throw("Invalid identifier name");

        var value = _valueExpression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return (name, value);
    }
}
