using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements.Arms;

namespace Bloc.Statements;

internal sealed class SwitchStatement : Statement
{
    internal required IExpression Expression { get; init; }
    internal required List<IArm> Arms { get; init; }
    internal required Statement? Default { get; init; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception!;
            yield break;
        }

        Statement? statement = Default;

        foreach (var arm in Arms)
        {
            if (arm.Matches(value!, call))
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

    public override int GetHashCode()
    {
        return HashCode.Combine(Expression, Default, Arms.Count);
    }

    public override bool Equals(object other)
    {
        return other is SwitchStatement statement &&
            Expression.Equals(statement.Expression) &&
            Equals(Default, statement.Default) &&
            Arms.SequenceEqual(statement.Arms);
    }
}