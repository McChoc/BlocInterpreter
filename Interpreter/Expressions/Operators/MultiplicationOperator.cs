using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record MultiplicationOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal MultiplicationOperator(IExpression left, IExpression right)
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
            (INumeric left, INumeric right) => MultiplyNumerics(left, right),
            (String @string, INumeric numeric) => MultiplyString(@string, numeric),
            (INumeric numeric, String @string) => MultiplyString(@string, numeric),
            (Array array, INumeric numeric) => MultiplyArray(array, numeric),
            (INumeric numeric, Array array) => MultiplyArray(array, numeric),

            _ => throw new Throw($"Cannot apply operator '*' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number MultiplyNumerics(INumeric left, INumeric right)
    {
        return new Number(left.GetDouble() * right.GetDouble());
    }

    private static String MultiplyString(String @string, INumeric numeric)
    {
        int count = numeric.GetInt();

        if (count < 0)
            throw new Throw("You cannot multiply a string by a negative number");

        var builder = new StringBuilder(@string.Value.Length * count);

        for (var i = 0; i < count; i++)
            builder.Append(@string.Value);

        return new String(builder.ToString());
    }

    private static Array MultiplyArray(Array array, INumeric numeric)
    {
        int count = numeric.GetInt();

        if (count < 0)
            throw new Throw("You cannot multiply an array by a negative number");

        var list = new List<Value>(array.Values.Count * count);

        for (var i = 0; i < count; i++)
            list.AddRange(array.Values.Select(x => x.Value.GetOrCopy()));

        return new Array(list);
    }
}