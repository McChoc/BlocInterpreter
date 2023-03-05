using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class EmptyStatement : Statement
{
    internal override IEnumerable<IResult> Execute(Call _)
    {
        yield break;
    }
}