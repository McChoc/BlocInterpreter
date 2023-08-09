using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

internal sealed record LessThanPatternLiteral : IExpression
{
    private readonly IExpression _expression;

    public LessThanPatternLiteral(IExpression expression)
    {
        _expression = expression;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;
        var pattern = new LessThanPattern(value);

        return new Pattern(pattern);
    }
}