using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements;

internal sealed class ForStatement : Statement
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

    internal override IEnumerable<Result> Execute(Call call)
    {
        using (call.MakeScope())
        {
            if (Initialisation is not null)
            {
                if (!EvaluateExpression(Initialisation, call, out var _, out var exception))
                {
                    yield return exception!;
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
                        yield return exception!;
                        yield break;
                    }

                    if (!Bool.TryImplicitCast(value!.Value, out var @bool))
                    {
                        yield return new Throw("Cannot implicitly convert to bool");
                        yield break;
                    }

                    if (!@bool.Value)
                        break;
                }

                if (++loopCount > call.Engine.LoopLimit && _checked)
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

                if (Increment is not null)
                {
                    if (!EvaluateExpression(Increment, call, out var _, out var exception))
                    {
                        yield return exception!;
                        yield break;
                    }
                }
            }
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _checked, Initialisation, Condition, Increment, Statement);
    }

    public override bool Equals(object other)
    {
        return other is ForStatement statement &&
            Label == statement.Label &&
            _checked == statement._checked &&
            Equals(Initialisation, statement.Initialisation) &&
            Equals(Condition, statement.Condition) &&
            Equals(Increment, statement.Increment) &&
            Statement == statement.Statement;
    }
}