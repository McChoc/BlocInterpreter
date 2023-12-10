using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Cases;
using Bloc.Utils.Attributes;

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

        var cases = Cases
            .Select(x => x.Evaluate(call))
            .ToArray();

        var statement = Default;

        foreach (var @case in cases)
        {
            if (@case.Matches(value, call))
            {
                statement = @case.Statement;
                break;
            }
        }

        if (statement is null)
            yield break;

        bool @continue;

        do
        {
            @continue = false;

            using (call.MakeScope())
            {
                foreach (var result in ExecuteStatement(statement, call))
                {
                    switch (result)
                    {
                        case GotoCase gotoCase:
                            foreach (var @case in cases)
                            {
                                if (@case.Value != gotoCase.Value)
                                    continue;

                                if (@case.JumpCount++ >= call.Engine.Options.JumpLimit)
                                {
                                    yield return new Throw("The jump limit was reached.");
                                    yield break;
                                }

                                statement = @case.Statement;
                                @continue = true;
                                break;
                            }

                            if (!@continue)
                            {
                                yield return new Throw($"case ({gotoCase.Value}) does not exist in this statement.");
                                yield break;
                            }

                            break;

                        case Yield:
                            yield return result;
                            break;

                        default:
                            yield return result;
                            yield break;
                    }
                }
            }
        }
        while (@continue);
    }
}