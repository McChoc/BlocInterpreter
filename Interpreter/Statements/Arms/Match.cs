using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Statements.Arms;

internal sealed record Match : IArm
{
    private readonly IExpression _expression;

    public Statement Statement { get; set; }

    public Match(IExpression expression, Statement statement)
    {
        _expression = expression;
        Statement = statement;
    }

    public bool Matches(Value value, Call call)
    {
        var pattern = GetPattern(_expression, call);

        return pattern.Matches(value, call);
    }

    private static IPatternNode GetPattern(IExpression expression, Call call)
    {
        var value = expression.Evaluate(call).Value;

        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not IPattern pattern)
            throw new Throw($"The expression of a match arm must be a pattern");

        return pattern.GetRoot();
    }
}