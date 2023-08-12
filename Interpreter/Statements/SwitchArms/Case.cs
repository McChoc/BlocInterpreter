using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Statements.SwitchArms;

internal sealed record Case : IArm
{
    private readonly IExpression _expression;

    public Statement Statement { get; set; }

    public Case(IExpression expression, Statement statement)
    {
        _expression = expression;
        Statement = statement;
    }

    public bool Matches(Value value, Call call)
    {
        return value == _expression.Evaluate(call).Value;
    }
}