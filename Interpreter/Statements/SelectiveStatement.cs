using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Cases;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using System.Diagnostics.CodeAnalysis;

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

        int defaultJumpCount = 0;

        var cases = Cases
            .Select(x => x.Evaluate(call))
            .ToArray();

        var statement = GetMatchingStatement(value, cases, call);

        while (statement is not null)
        {
            using (call.MakeScope())
            {
                var results = ExecuteStatement(statement, call);
                statement = null;

                foreach (var result in results)
                {
                    switch (result)
                    {
                        case GotoDefault when Default is not null:
                            if (defaultJumpCount++ < call.Engine.Options.JumpLimit)
                            {
                                statement = Default;
                                break;
                            }
                            else
                            {
                                yield return new Throw("The jump limit was reached.");
                                yield break;
                            }

                        case GotoCase gotoCase when TryGetCase(gotoCase.Value, cases, out var @case):
                            if (@case.JumpCount++ < call.Engine.Options.JumpLimit)
                            {
                                statement = @case.Statement;
                                break;
                            }
                            else
                            {
                                yield return new Throw("The jump limit was reached.");
                                yield break;
                            }

                        case Yield:
                            yield return result;
                            break;

                        default:
                            yield return result;
                            yield break;
                    }

                    if (statement is not null)
                        break;
                }
            }
        }
    }

    private Statement? GetMatchingStatement(Value value, CaseInfo[] cases, Call call)
    {
        foreach (var @case in cases)
            if (@case.Matches(value, call))
                return @case.Statement;

        return Default;
    }

    private bool TryGetCase(Value value, CaseInfo[] cases, [NotNullWhen(true)] out CaseInfo? @case)
    {
        foreach (var c in cases)
        {
            if (c.Value.Equals(value))
            {
                @case = c;
                return true;
            }
        }

        @case = null;
        return false;
    }
}