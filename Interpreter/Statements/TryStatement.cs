using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
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
                    if (!EvaluateExpression(@catch.Name, call, out var identifier, out var exception))
                    {
                        yield return exception!;
                        yield break;
                    }

                    VariableHelper.Define(identifier!, @throw.Value.Copy(), call);

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

    internal sealed class Catch
    {
        internal IExpression Name { get; set; }

        internal IExpression? Expression { get; set; }

        internal Statement Statement { get; set; }

        internal Catch(IExpression name, IExpression? expression, Statement statement)
        {
            Name = name;
            Expression = expression;
            Statement = statement;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Expression, Statement);
        }

        public override bool Equals(object other)
        {
            return other is Catch @catch &&
                Name == @catch.Name &&
                Equals(Expression, @catch.Expression) &&
                Statement == @catch.Statement;
        }
    }
}