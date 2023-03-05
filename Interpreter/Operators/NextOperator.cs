﻿using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record NextOperator : IExpression
{
    private readonly IExpression _operand;

    internal NextOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is Iter iter)
            return iter.Next();

        throw new Throw($"Cannot apply operator 'next' on type {value.GetTypeName()}");
    }
}