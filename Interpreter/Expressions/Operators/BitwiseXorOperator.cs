using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record BitwiseXorOperator : IExpression
{
    private readonly IExpression _left;
    private readonly IExpression _right;

    internal BitwiseXorOperator(IExpression left, IExpression right)
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
            (INumeric left, INumeric right) => XorScalars(left, right),
            (IPattern left, IPattern right) => XorPatterns(left, right),

            _ => throw new Throw($"Cannot apply operator '^' on operands of types {a.GetTypeName()} and {b.GetTypeName()}"),
        };
    }

    private static Number XorScalars(INumeric left, INumeric right)
    {
        return new Number(left.GetInt() ^ right.GetInt());
    }

    private static Pattern XorPatterns(IPattern left, IPattern right)
    {
        return new Pattern(new XorPattern(left.GetRoot(), right.GetRoot()));
    }
}