﻿using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.SubExpressions;

internal sealed record Element(IExpression Expression) : IElement
{
    public IEnumerable<Value> GetElements(Call call)
    {
        var value = Expression.Evaluate(call).Value.GetOrCopy();

        if (value is Void)
            throw new Throw("'void' is not assignable");

        yield return value;
    }
}