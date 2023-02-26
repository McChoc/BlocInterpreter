using System;
using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class GotoStatement : Statement
{
    private readonly string _label;

    internal GotoStatement(string label)
    {
        _label = label;
    }

    internal override IEnumerable<Result> Execute(Call call)
    {
        yield return new Goto(_label);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _label);
    }

    public override bool Equals(object other)
    {
        return other is GotoStatement statement &&
            Label == statement.Label &&
            _label == statement._label;
    }
}