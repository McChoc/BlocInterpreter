using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class EmptyStatement : Statement
{
    internal override IEnumerable<IResult> Execute(Call _)
    {
        yield break;
    }
}