using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class GotoStatement : Statement
{
    private readonly string _label;

    internal GotoStatement(string label)
    {
        _label = label;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        yield return new Goto(_label);
    }
}