using System;
using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class LoopStatement : Statement
{
    private readonly bool _checked;

    internal required Statement Statement { get; init; }

    internal LoopStatement(bool @checked)
    {
        _checked = @checked;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        int loopCount = 0;

        while (true)
        {
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
        return HashCode.Combine(Label, _checked, Statement);
    }

    public override bool Equals(object other)
    {
        return other is LoopStatement statement &&
            Label == statement.Label &&
            _checked == statement._checked &&
            Statement == statement.Statement;
    }
}