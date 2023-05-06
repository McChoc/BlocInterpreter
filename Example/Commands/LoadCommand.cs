using System.Collections.Generic;
using System.IO;
using Bloc.Commands;
using Bloc.Core;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Exceptions;
using Bloc.Values;

namespace ConsoleApp.Commands;

public class LoadCommand : ICommandInfo
{
    public string Name => "load";

    public string Description =>
        """
        load <path>
        Loads and execute the code stored in a text file.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length != 1)
            throw new Throw($"'load' does not take {args.Length} arguments.\nType '/help load' to see its usage");

        if (!File.Exists(args[0]))
            throw new Throw("File does not exists");

        var code = File.ReadAllText(args[0]);

        List<Statement> statements;

        try
        {
            Engine.Compile(code, out var _, out statements);
        }
        catch (SyntaxError e)
        {
            throw new Throw($"Syntax error : {e.Message}");
        }

        var result = call.Engine.Execute(statements);

        if (result is not null)
            throw result;

        return Void.Value;
    }
}