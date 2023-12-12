using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class ForStatement : Statement
{
    private readonly bool _checked;

    internal required IExpression? Initialisation { get; init; }
    internal required IExpression? Condition { get; init; }
    internal required IExpression? Increment { get; init; }
    internal required Statement Statement { get; init; }

    internal ForStatement(bool @checked)
    {
        _checked = @checked;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        using (call.MakeScope())
        {
            if (Initialisation is not null)
            {
                if (!EvaluateExpression(Initialisation, call, out var _, out var exception))
                {
                    yield return exception;
                    yield break;
                }
            }

            int loopCount = 0;

            while (true)
            {
                if (Condition is not null)
                {
                    if (!EvaluateExpression(Condition, call, out var value, out var exception))
                    {
                        yield return exception;
                        yield break;
                    }

                    if (!Bool.TryImplicitCast(value, out var @bool))
                    {
                        yield return new Throw("Cannot implicitly convert to bool");
                        yield break;
                    }

                    if (!@bool.Value)
                        break;
                }

                if (++loopCount > call.Engine.Options.LoopLimit && _checked)
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
                            case Break { Label: null }:
                            case Break { Label: string label } when label == Label:
                                @break = true;
                                break;

                            case Continue { Label: null }:
                            case Continue { Label: string label } when label == Label:
                                @continue = true;
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

                if (Increment is not null)
                {
                    if (!EvaluateExpression(Increment, call, out var _, out var exception))
                    {
                        yield return exception;
                        yield break;
                    }
                }
            }
        }
    }
}