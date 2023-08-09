using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

internal sealed record GreaterThanPatternLiteral : IExpression
{
    private readonly IExpression _expression;

    public GreaterThanPatternLiteral(IExpression expression)
    {
        _expression = expression;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;
        var pattern = new GreaterThanPattern(value);

        return new Pattern(pattern);
    }
}