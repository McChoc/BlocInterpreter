using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Switch;

internal sealed record Case : IArm
{
    private readonly IExpression _expression;

    public IExpression Expression { get; set; }

    public Case(IExpression comparedExpression, IExpression resultExpression)
    {
        _expression = comparedExpression;
        Expression = resultExpression;
    }

    public bool Matches(Value value, Call call)
    {
        return value == _expression.Evaluate(call).Value;
    }
}