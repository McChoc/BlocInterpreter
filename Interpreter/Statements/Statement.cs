using System.Collections.Generic;
using Bloc.Commands;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Statements;

public abstract class Statement
{
    internal string? Label { get; set; }

    internal abstract IEnumerable<IResult> Execute(Call call);

    private protected static bool DefineIdentifier(IIdentifier identifier, Value value, Call call, out Throw? exception, bool mask = false, bool mutable = true)
    {
        try
        {
            identifier.Define(value, call, mask, mutable);
            exception = null;
            return true;
        }
        catch (Throw e)
        {
            exception = e;
            return false;
        }
    }

    private protected static bool EvaluateExpression(IExpression expression, Call call, out Value? value, out Throw? exception)
    {
        try
        {
            value = expression.Evaluate(call).Value;
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

    private protected static bool ExecuteCommand(Command command, Call call, out Value? value, out Throw? exception)
    {
        try
        {
            value = command.Execute(call);
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

    private protected static IEnumerable<IResult> ExecuteStatement(Statement statement, Call call)
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
                        if (jumpCount++ < call.Engine.Options.JumpLimit)
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

    private protected static IEnumerable<IResult> ExecuteStatements(List<Statement> statements, Call call)
    {
        var labels = StatementHelper.GetLabels(statements);

        for (var i = 0; i < statements.Count; i++)
        {
            foreach (var result in statements[i].Execute(call))
            {
                bool done = false;

                switch (result)
                {
                    case Goto @goto when labels.TryGetValue(@goto.Label, out var label):
                        if (label.Count++ < call.Engine.Options.JumpLimit)
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