using System.Collections.Generic;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class ContinueStatement : Statement
{
    private readonly INamedIdentifier? _identifier;

    internal ContinueStatement() { }

    internal ContinueStatement(INamedIdentifier? identifier)
    {
        _identifier = identifier;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        string? label = _identifier?.GetName(call);
        yield return new Continue(label);
    }
}