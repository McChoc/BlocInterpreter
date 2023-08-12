using System.Collections.Generic;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class GotoStatement : Statement
{
    private readonly INamedIdentifier _identifier;

    internal GotoStatement(INamedIdentifier identifier)
    {
        _identifier = identifier;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        var label = _identifier.GetName(call);

        yield return new Goto(label);
    }
}