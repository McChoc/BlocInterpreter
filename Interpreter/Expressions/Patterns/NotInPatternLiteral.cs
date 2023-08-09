using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Patterns;

internal sealed record NotInPatternLiteral : IExpression
{
    private readonly IExpression _expression;

    public NotInPatternLiteral(IExpression expression)
    {
        _expression = expression;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;
        var pattern = new NotInPattern(value);

        return new Pattern(pattern);
    }
}