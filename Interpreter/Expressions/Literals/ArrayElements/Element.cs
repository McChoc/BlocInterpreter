using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.ArrayElements;

internal sealed record Element : IElement
{
    private readonly IExpression _expression;

    public Element(IExpression expression)
    {
        _expression = expression;
    }

    public IEnumerable<Value> GetElements(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return value;
    }
}