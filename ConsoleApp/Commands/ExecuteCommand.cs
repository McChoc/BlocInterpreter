using System.Collections.Generic;
using Bloc.Commands;
using Bloc.Core;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Exceptions;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class ExecuteCommand : ICommandInfo
{
    public string Name => "execute";

    public string Description =>
        """
        execute <code>
        Executes a piece of code.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length != 1)
            throw new Throw($"'execute' does not take {args.Length} arguments.\nType '/help execute' to see its usage");

        if (args[0] is not String @string)
            throw new Throw("The code was not a string");

        List<Statement> statements;

        try
        {
            Engine.Compile(@string.Value, out var _, out statements);
        }
        catch (SyntaxError e)
        {
            throw new Throw($"Syntax error : {e.Message}");
        }

        var result = call.Engine.Execute(statements, call.Module);

        if (result is not null)
            throw result;

        return Void.Value;
    }
}