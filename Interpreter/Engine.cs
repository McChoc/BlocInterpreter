using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Commands;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Parsers;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc;

public sealed partial class Engine
{
    public Call GlobalCall { get; }
    public Scope GlobalScope { get; }

    internal int StackLimit { get; private set; }
    internal int LoopLimit { get; private set; }
    internal int JumpLimit { get; private set; }
    internal int HopLimit { get; private set; }

    public required Action<string> Log { get; init; }
    public required Action Clear { get; init; }
    public required Action Exit { get; init; }

    public required Dictionary<string, CommandInfo> Commands { get; init; }

    private Engine()
    {
        GlobalCall = new Call(this);
        GlobalScope = GlobalCall.Scopes.First();
    }

    public static void Compile(string code, out IExpression? expression, out List<Statement> statements)
    {
        var tokenizer = new Tokenizer(code);
        statements = StatementParser.Parse(tokenizer);

        expression = statements is [ExpressionStatement statement]
            ? statement.Expression
            : null;
    }

    public Throw? Evaluate(IExpression expression, out Value? value)
    {
        try
        {
            value = expression.Evaluate(GlobalCall).Value;
            return null;
        }
        catch (Throw @throw)
        {
            value = null;
            return @throw;
        }
    }

    public Throw? Execute(List<Statement> statements)
    {
        var labels = StatementHelper.GetLabels(statements);

        for (var i = 0; i < statements.Count; i++)
        {
            switch (statements[i].Execute(GlobalCall).FirstOrDefault())
            {
                case Continue:
                    throw new Throw("A continue statement can only be used inside a loop");

                case Break:
                    throw new Throw("A break statement can only be used inside a loop");

                case Yield:
                    throw new Throw("A yield statement can only be used inside a generator");

                case Return:
                    throw new Throw("A return statement can only be used inside a function");

                case Throw @throw:
                    return @throw;

                case Goto @goto:
                    if (labels.TryGetValue(@goto.Label, out var label))
                    {
                        if (++label.Count > JumpLimit)
                            return new Throw("The jump limit was reached.");

                        i = label.Index - 1;

                        continue;
                    }

                    return new Throw($"Label '{@goto.Label}' does not exist in scope.");
            }
        }

        return null;
    }
}