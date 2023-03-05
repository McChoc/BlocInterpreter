using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Variables;

namespace Bloc.Statements;

internal sealed class LockStatement : Statement
{
    internal required IExpression Expression { get; init; }
    internal required Statement Statement { get; init; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!EvaluateExpression(Expression, call, out var value, out var exception))
        {
            yield return exception!;
            yield break;
        }

        Variable variable;

        switch (value)
        {
            case UnresolvedPointer pointer:
                variable = pointer.Resolve().Variable!;
                break;

            case VariablePointer { Variable: not null } pointer:
                variable = pointer.Variable;
                break;

            case SlicePointer { Variables: not null }:
                yield return new Throw("You cannot lock a slice");
                yield break;

            case VariablePointer or SlicePointer:
                yield return new Throw("Invalid reference");
                yield break;

            default:
                yield return new Throw("You can only lock a variable");
                yield break;
        }

        lock (variable)
        {
            using (call.MakeScope())
            {
                foreach (var result in ExecuteStatement(Statement, call))
                {
                    yield return result;

                    if (result is not Yield)
                        yield break;
                }
            }
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Expression, Statement);
    }

    public override bool Equals(object other)
    {
        return other is LockStatement statement &&
            Label == statement.Label &&
            Expression.Equals(statement.Expression) &&
            Statement == statement.Statement;
    }
}