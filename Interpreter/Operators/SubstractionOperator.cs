using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Operators;

internal sealed record SubstractionOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal SubstractionOperator(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public IValue Evaluate(Call call)
    {
        var left = _left.Evaluate(call).Value;
        var right = _right.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(left, right, Operation, call);
    }

    internal static Value Operation(Value a, Value b)
    {
        return (a, b) switch
        {
            (IScalar left, IScalar right)   => SubstractScalars(left, right),
            (Array array, Value value)      => RemoveFromArray(array, value),

            _ => throw new Throw($"Cannot apply operator '-' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number SubstractScalars(IScalar left, IScalar right)
    {
        return new Number(left.GetDouble() - right.GetDouble());
    }

    private static Array RemoveFromArray(Array array, Value value)
    {
        bool found = false;

        var list = new List<Value>(array.Values.Count - 1);

        foreach (var variable in array.Values)
        {
            if (!found && variable.Value.Equals(value))
                found = true;
            else
                list.Add(variable.Value.GetOrCopy());
        }

        return new Array(list);
    }
}