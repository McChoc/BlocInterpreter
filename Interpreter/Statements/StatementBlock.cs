using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class StatementBlock : Statement
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

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Statements.Count);
    }

    public override bool Equals(object other)
    {
        return other is StatementBlock block &&
            Label == block.Label &&
            Statements.SequenceEqual(block.Statements);
    }
}