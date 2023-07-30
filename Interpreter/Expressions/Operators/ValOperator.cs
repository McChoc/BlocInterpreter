﻿using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record ValOperator : IExpression
{
    private readonly IExpression _operand;

    internal ValOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        if (value is not Reference reference)
            throw new Throw("The 'val' operator can only be used on references");

        return reference.Pointer;
    }
}