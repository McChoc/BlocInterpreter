using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;

namespace Bloc.Statements;

[Record]
internal sealed partial class SelectiveStatement : Statement
{
    internal required IExpression Expression { get; init; }
    internal required List<Case> Cases { get; init; }
    internal required Statement? Default { get; init; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception;
            yield break;
        }

        var statement = Default;

        foreach (var arm in Cases)
        {
            if (arm.Matches(value, call))
            {
                statement = arm.Statement;
                break;
            }
        }

        if (statement is null)
            yield break;

        using (call.MakeScope())
        {
            foreach (var result in ExecuteStatement(statement, call))
            {
                yield return result;

                if (result is not Yield)
                    yield break;
            }
        }
    }

    internal abstract record Case
    {
        public IExpression Expression { get; set; } = null!;
        public Statement Statement { get; set; } = null!;

        public abstract bool Matches(Value value, Call call);
    }

    internal sealed record SwitchCase : Case
    {
        public override bool Matches(Value value, Call call)
        {
            return Expression.Evaluate(call).Value.Equals(value);
        }
    }

    internal sealed record MatchCase : Case
    {
        public override bool Matches(Value value, Call call)
        {
            return GetPattern(Expression, call).Matches(value, call);
        }

        private static IPatternNode GetPattern(IExpression expression, Call call)
        {
            var value = expression.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

            if (value is not IPattern pattern)
                throw new Throw($"The expression of a case arm must be a pattern");

            return pattern.GetRoot();
        }
    }
}