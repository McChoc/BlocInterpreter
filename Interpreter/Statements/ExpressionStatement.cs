using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;

namespace Bloc.Statements;

internal sealed class ExpressionStatement : Statement
{
    internal IExpression Expression { get; }

    internal ExpressionStatement(IExpression expression)
    {
        Expression = expression;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var _, out var exception))
            yield return exception!;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Expression);
    }

    public override bool Equals(object other)
    {
        return other is ExpressionStatement statement &&
            Label == statement.Label &&
            Expression.Equals(statement.Expression);
    }
}