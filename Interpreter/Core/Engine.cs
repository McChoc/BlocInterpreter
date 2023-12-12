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
using Bloc.Values.Core;

namespace Bloc.Core;

public sealed class Engine
{
    public object? State { get; set; }
    public IEngineOptions Options { get; }
    public Action<string> Output { get; }
    public Dictionary<string, string> Aliases { get; }
    public Dictionary<string, ICommandInfo> Commands { get; }
    public Dictionary<string, Module> Modules { get; }

    internal VariableCollection GlobalVariables { get; }

    internal Engine(IEngineOptions options, Action<string> output, Dictionary<string, string> aliases, Dictionary<string, ICommandInfo> commands)
    {
        Options = options;
        Output = output;
        Aliases = aliases;
        Commands = commands;
        Modules = new();
        GlobalVariables = new();
    }

    public static void Compile(string code, out IExpression? expression, out List<Statement> statements)
    {
        var tokenizer = new Tokenizer(code);
        statements = StatementParser.Parse(tokenizer);

        expression = statements is [ExpressionStatement statement]
            ? statement.Expression
            : null;
    }

    public Throw? Evaluate(IExpression expression, Module module, out Value? value)
    {
        try
        {
            value = expression.Evaluate(module.TopLevelCall).Value.GetOrCopy();
            return null;
        }
        catch (Throw @throw)
        {
            value = null;
            return @throw;
        }
    }

    public Throw? Execute(List<Statement> statements, Module module)
    {
        var labels = StatementHelper.GetLabels(statements);

        for (int i = 0; i < statements.Count; i++)
        {
            switch (statements[i].Execute(module.TopLevelCall).FirstOrDefault())
            {
                case Return:
                    return new Throw("A return statement can only be used inside a function");

                case Yield:
                    return new Throw("A yield statement can only be used inside a generator function");

                case Break:
                    return new Throw("A break statement can only be used inside a loop");

                case Continue:
                    return new Throw("A continue statement can only be used inside a loop");

                case GotoCase:
                    return new Throw("A goto case statement can only be used inside a switch or a match statement");

                case GotoDefault:
                    return new Throw("A goto default statement can only be used inside a switch or a match statement");

                case Throw @throw:
                    return @throw;

                case Goto @goto:
                    if (labels.TryGetValue(@goto.Label, out var label))
                    {
                        if (++label.JumpCount > Options.JumpLimit)
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