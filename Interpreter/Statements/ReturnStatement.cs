using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class ReturnStatement : Statement
{
    private readonly IExpression? _expression;

    internal ReturnStatement() { }

    internal ReturnStatement(IExpression expression)
    {
        _expression = expression;
    }

    internal override IEnumerable<Result> Execute(Call call)
    {
        if (_expression is null)
            yield return new Return();
        else if (EvaluateExpression(_expression, call, out var value, out var exception))
            yield return new Return(value!.Value);
        else
            yield return exception!;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, _expression);
    }

    public override bool Equals(object other)
    {
        return other is ReturnStatement statement &&
            Label == statement.Label &&
            Equals(_expression, statement._expression);
    }
}