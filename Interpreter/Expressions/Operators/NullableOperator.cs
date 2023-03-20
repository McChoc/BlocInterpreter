using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions.Operators;

internal sealed record NullableOperator : IExpression
{
    private readonly IExpression _operand;

    internal NullableOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

        if (value is Type type)
        {
            var types = new HashSet<ValueType>
            {
                ValueType.Null
            };

            foreach (var t in type.Value)
                types.Add(t);

            return new Type(types);
        }

        throw new Throw($"Cannot apply operator '?' on type {value.GetTypeName()}");
    }
}