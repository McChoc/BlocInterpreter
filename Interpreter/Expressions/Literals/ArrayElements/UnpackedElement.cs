using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.ArrayElements;

internal sealed record UnpackedElement : IElement
{
    private readonly IExpression _expression;

    public UnpackedElement(IExpression expression)
    {
        _expression = expression;
    }

    public IEnumerable<Value> GetElements(Call call)
    {
        var value = _expression.Evaluate(call).Value;

        if (!Iter.TryImplicitCast(value, out var iter, call))
            throw new Throw("Cannot implicitly convert to iter");

        return IterHelper.CheckedIterate(iter, call.Engine.Options);
    }
}