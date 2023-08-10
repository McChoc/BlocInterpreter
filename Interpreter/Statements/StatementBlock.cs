using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class StatementBlock : Statement
{
    internal required List<Statement> Statements { get; set; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        using (call.MakeScope())
        {
            foreach (var result in ExecuteStatements(Statements, call))
            {
                yield return result;

                if (result is not Yield)
                    yield break;
            }
        }
    }
}