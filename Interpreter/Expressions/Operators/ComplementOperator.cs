using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record ComplementOperator : IExpression
{
    private readonly IExpression _operand;

    internal ComplementOperator(IExpression operand)
    {
        _operand = operand;
    }

    public IValue Evaluate(Call call)
    {
        var value = _operand.Evaluate(call).Value;

        return OperatorHelper.RecursivelyCall(value, Operation, call);
    }

    internal static Value Operation(Value value)
    {
        return value switch
        {
            INumeric numeric => ComplementNumeric(numeric),
            IPattern pattern => ComplementPattern(pattern),

            _ => throw new Throw($"Cannot apply operator '~' on type {value.GetTypeName()}")
        };
    }

    private static Number ComplementNumeric(INumeric numeric)
    {
        return new Number(~numeric.GetInt());
    }

    private static Pattern ComplementPattern(IPattern pattern)
    {
        return new Pattern(new NotPattern(pattern.GetRoot()));
    }
}