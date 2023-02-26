using System;
using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements;

public abstract class Statement
{
    internal string? Label { get; set; }

    internal abstract IEnumerable<Result> Execute(Call call);

    public override int GetHashCode()
    {
        return HashCode.Combine(Label);
    }

    public override bool Equals(object other)
    {
        return other is Statement statement &&
            GetType() == statement.GetType() &&
            Label == statement.Label;
    }

    public static bool operator ==(Statement? a, Statement? b) => Equals(a, b);

    public static bool operator !=(Statement? a, Statement? b) => !Equals(a, b);

    private protected static bool EvaluateExpression(IExpression expression, Call call, out IValue? value, out Throw? exception)
    {
        try
        {
            value = expression.Evaluate(call);
            exception = null;
            return true;
        }
        catch (Throw e)
        {
            value = null;
            exception = e;
            return false;
        }
    }

    private protected static IEnumerable<Result> ExecuteStatement(Statement statement, Call call)
    {
        bool rerun;
        int jumpCount = 0;

        do
        {
            rerun = false;

            foreach (var result in statement.Execute(call))
            {
                switch (result)
                {
                    case Goto @goto when @goto.Label == statement.Label:
                        if (jumpCount++ < call.Engine.JumpLimit)
                        {
                            rerun = true;
                            break;
                        }
                        else
                        {
                            yield return new Throw("The jump limit was reached.");
                            yield break;
                        }

                    case Yield:
                        yield return result;
                        break;

                    default:
                        yield return result;
                        yield break;
                }

                if (rerun)
                    break;
            }
        }
        while (rerun);
    }

    private protected static IEnumerable<Result> ExecuteStatements(List<Statement> statements, Call call)
    {
        var labels = StatementUtil.GetLabels(statements);

        for (var i = 0; i < statements.Count; i++)
        {
            foreach (var result in statements[i].Execute(call))
            {
                bool done = false;

                switch (result)
                {
                    case Goto @goto when labels.TryGetValue(@goto.Label, out var label):
                        if (label.Count++ < call.Engine.JumpLimit)
                        {
                            i = label.Index - 1;
                            done = true;
                            break;
                        }
                        else
                        {
                            yield return new Throw("The jump limit was reached.");
                            yield break;
                        }

                    case Yield:
                        yield return result;
                        break;

                    default:
                        yield return result;
                        yield break;
                }

                if (done)
                    break;
            }
        }
    }
}