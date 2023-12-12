using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class LoopStatement : Statement
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