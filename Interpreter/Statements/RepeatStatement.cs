using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class RepeatStatement : Statement
{
    private readonly bool _checked;

    internal required IExpression Expression { get; init; }
    internal required Statement Statement { get; init; }

    internal RepeatStatement(bool @checked)
    {
        _checked = @checked;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception!;
            yield break;
        }

        if (!Number.TryImplicitCast(value!, out var number))
        {
            yield return new Throw("Cannot implicitly convert to number");
            yield break;
        }

        int loopCount = number.GetInt();

        for (int i = 0; i < loopCount; i++)
        {
            if (_checked && i >= call.Engine.Options.LoopLimit)
            {
                yield return new Throw("The loop limit was reached.");
                yield break;
            }

            bool @break = false;

            using (call.MakeScope())
            {
                foreach (var result in ExecuteStatement(Statement, call))
                {
                    bool @continue = false;

                    switch (result)
                    {
                        case Continue:
                            @continue = true;
                            break;

                        case Break:
                            @break = true;
                            break;

                        case Yield:
                            yield return result;
                            break;

                        default:
                            yield return result;
                            yield break;
                    }

                    if (@continue || @break)
                        break;
                }
            }

            if (@break)
                break;
        }
    }
}