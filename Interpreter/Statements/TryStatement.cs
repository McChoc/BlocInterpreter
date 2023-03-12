using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements;

internal sealed class TryStatement : Statement
{
    internal required Statement Try { get; init; }
    internal required List<Catch> Catches { get; init; }
    internal required Statement? Finally { get; init; }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        IResult? mainResult = null;

        using (call.MakeScope())
        {
            foreach (var result in ExecuteStatement(Try, call))
            {
                if (result is Yield)
                {
                    yield return result;
                }
                else
                {
                    mainResult = result;
                    break;
                }
            }
        }

        if (mainResult is Throw @throw)
        {
            foreach (var @catch in Catches)
            {
                using (call.MakeScope())
                {
                    if (@catch.Identifier is not null)
                    {
                        if (!DefineIdentifier(@catch.Identifier, @throw.Value.Copy(), call, out var exception))
                        {
                            yield return exception!;
                            yield break;
                        }
                    }

                    if (@catch.Expression is not null)
                    {
                        if (Bool.TryImplicitCast(@catch.Expression.Evaluate(call), out var @bool))
                        {
                            if (!@bool.Value)
                                continue;
                        }
                        else
                        {
                            mainResult = new Throw("Cannot implicitly convert to bool");
                            break;
                        }
                    }

                    mainResult = null;

                    foreach (var result in ExecuteStatement(@catch.Statement, call))
                    {
                        if (result is Yield)
                        {
                            yield return result;
                        }
                        else
                        {
                            mainResult = result;
                            break;
                        }
                    }

                    break;
                }
            }
        }

        if (Finally is not null)
        {
            using (call.MakeScope())
            {
                foreach (var result in ExecuteStatement(Finally, call))
                {
                    if (result is Yield)
                    {
                        yield return result;
                    }
                    else
                    {
                        mainResult = result;
                        break;
                    }
                }
            }
        }

        if (mainResult is not null)
            yield return mainResult;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Try, Catches.Count, Finally);
    }

    public override bool Equals(object other)
    {
        return other is TryStatement statement &&
            Label == statement.Label &&
            Try == statement.Try &&
            Catches.SequenceEqual(statement.Catches) &&
            Finally == statement.Finally;
    }

    internal sealed record Catch(IIdentifier? Identifier, IExpression? Expression, Statement Statement);
}