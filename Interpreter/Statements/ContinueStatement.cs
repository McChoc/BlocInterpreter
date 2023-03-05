using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class ContinueStatement : Statement
{
    internal override IEnumerable<IResult> Execute(Call call)
    {
        yield return new Continue();
    }
}