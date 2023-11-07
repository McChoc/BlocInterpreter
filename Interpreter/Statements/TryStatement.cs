using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class TryStatement : Statement
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
                            yield return exception;
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

    internal sealed record Catch(IIdentifier? Identifier, IExpression? Expression, Statement Statement);
}