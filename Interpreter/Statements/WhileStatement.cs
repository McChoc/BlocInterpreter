using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class WhileStatement : Statement
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

    internal override IEnumerable<IResult> Execute(Call call)
    {
        int loopCount = 0;

        while (true)
        {
            if (!_executeAtLeastOnce || loopCount != 0)
            {
                if (!EvaluateExpression(Expression, call, out var value, out var exception))
                {
                    yield return exception;
                    yield break;
                }

                if (!Bool.TryImplicitCast(value, out var @bool))
                {
                    yield return new Throw("Cannot implicitly convert to bool");
                    yield break;
                }

                if (@bool.Value == _reversed)
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
        }
    }
}