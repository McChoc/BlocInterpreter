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
internal sealed partial class SwitchStatement : Statement
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

    internal sealed record Case(IExpression Expression, Statement Statement)
    {
        public bool Matches(Value value, Call call)
        {
            var pattern = GetPattern(Expression, call);

            return pattern.Matches(value, call);
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