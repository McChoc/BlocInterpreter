using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements;

internal sealed class IfStatement : Statement
{
    private readonly bool _reversed;

    internal required IExpression Expression { get; init; }
    internal required Statement Then { get; init; }
    internal required Statement? Else { get; init; }

    internal IfStatement(bool reversed)
    {
        _reversed = reversed;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception!;
            yield break;
        }

        if (!Bool.TryImplicitCast(value!, out var @bool))
        {
            yield return new Throw("Cannot implicitly convert to bool");
            yield break;
        }

        var statement = @bool.Value != _reversed
            ? Then
            : Else;

        if (statement is null)
            yield break;

        using (call.MakeScope())
        {
            foreach (var result in ExecuteStatement(statement, call))
            {
                yield return result;

                if (result is not Yield)
                    yield break;
            }
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Expression, Then, Else);
    }

    public override bool Equals(object other)
    {
        return other is IfStatement statement &&
            Label == statement.Label &&
            Expression.Equals(statement.Expression) &&
            Then == statement.Then &&
            Else == statement.Else;
    }
}