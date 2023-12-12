using System.Collections.Generic;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class BreakStatement : Statement
{
    private readonly INamedIdentifier? _identifier;

    internal BreakStatement() { }

    internal BreakStatement(INamedIdentifier? identifier)
    {
        _identifier = identifier;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        string? label = _identifier?.GetName(call);
        yield return new Break(label);
    }
}