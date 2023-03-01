using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements;

internal sealed class WhileStatement : Statement
{
    private readonly bool _checked;
    private readonly bool _reversed;
    private readonly bool _executeAtLeastOnce;

    internal required IExpression Expression { get; set; }
    internal required Statement Statement { get; set; }

    internal WhileStatement(bool @checked, bool reversed, bool executeAtLeastOnce)
    {
        _checked = @checked;
        _reversed = reversed;
        _executeAtLeastOnce = executeAtLeastOnce;
    }

    internal override IEnumerable<Result> Execute(Call call)
    {
        int loopCount = 0;

        while (true)
        {
            if (!_executeAtLeastOnce || loopCount != 0)
            {
                if (!EvaluateExpression(Expression, call, out var value, out var exception))
                {
                    yield return exception!;
                    yield break;
                }

                if (!Bool.TryImplicitCast(value!.Value, out var @bool))
                {
                    yield return new Throw("Cannot implicitly convert to bool");
                    yield break;
                }

                if (@bool.Value == _reversed)
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
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _checked, _reversed, _executeAtLeastOnce, Expression, Statement);
    }

    public override bool Equals(object other)
    {
        return other is WhileStatement statement &&
            Label == statement.Label &&
            _checked == statement._checked &&
            _reversed == statement._reversed &&
            _executeAtLeastOnce == statement._executeAtLeastOnce &&
            Expression.Equals(statement.Expression) &&
            Statement == statement.Statement;
    }
}