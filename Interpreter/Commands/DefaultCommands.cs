using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Exceptions;
using Bloc.Values;
using String = Bloc.Values.String;
using Void = Bloc.Values.Void;

namespace Bloc.Commands;

public static class DefaultCommands
{
    private static readonly Random rng = new();

    public static CommandInfo Help => new(
        "help",
        """
        help
        Returns a list of all commands.
        
        help <command_name>
        <command_name: string> |> help
        Returns the description of the given command.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
            {
                if (input is Void)
                {
                    var message =
                        "For more informations on a specific command, type '/help <command>'.\n" +
                        "\n" +
                        string.Join("\n", call.Engine.Commands.Values.Select(c => c.Name.ToLower()));

                    return new String(message);
                }

                if (input is String @string)
                {
                    if (!call.Engine.Commands.TryGetValue(@string.Value.ToLower(), out var command))
                        throw new Throw("Unknown command");

                    return new String(command.Description);
                }

                throw new Throw("The input could not be converted to a string");
            }

            if (args.Length == 1)
            {
                if (!call.Engine.Commands.TryGetValue(args[0].ToLower(), out var command))
                    throw new Throw("Unknown command");

                return new String(command.Description);
            }

            throw new Throw($"'help' does not take {args.Length} arguments.\nType '/help help' to see its usage");
        });

    public static CommandInfo Echo => new(
        "echo",
        """
        echo <message>
        <message: string> |> echo
        Returns the message.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
                return input as String ?? throw new Throw("The input was not a string");

            if (args.Length == 1)
                return new String(args[0]);

            throw new Throw($"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage");
        });

    public static CommandInfo Clear => new(
        "clear",
        """
        clear
        Clears the console.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length != 0)
                throw new Throw("'clear' does not take arguments.\nType '/help clear' to see its usage");

            call.Engine.Clear();

            return Void.Value;
        });

    public static CommandInfo Exit => new(
        "exit",
        """
        exit
        Exits the application.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length != 0)
                throw new Throw("'exit' does not take arguments.\nType '/help exit' to see its usage.");

            call.Engine.Exit();

            return Void.Value;
        });

    public static CommandInfo Get => new(
        "get",
        """
        get <name>
        <name: string> |> get
        Gets the value of the variable with the specified name.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
            {
                if (input is not String @string)
                    throw new Throw("The input was not a 'string'");

                return call.Get(@string.Value).Get();
            }

            if (args.Length == 1)
                return call.Get(args[0]).Get();

            throw new Throw($"'get' does not take {args.Length} arguments.\nType '/help get' to see its usage");
        });

    public static CommandInfo Set => new(
        "set",
        """
        set <name> <value>
        <value> |> set <name>
        Creates a new variable with a specified name and value in the current scope.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
                throw new Throw("'set' does not take 0 arguments.\nType '/help set' to see its usage");

            if (args.Length == 1)
            {
                if (input is Void)
                    throw new Throw("The input was empty");

                var name = args[0];
                var value = input;

                call.Set(true, true, name, value.GetOrCopy(true));

                return Void.Value;
            }

            if (args.Length == 2)
            {
                var name = args[0];
                var value = new String(args[1]);

                call.Set(true, true, name, value.GetOrCopy(true));

                return Void.Value;
            }

            throw new Throw($"'set' does not take {args.Length} arguments.\nType '/help set' to see its usage");
        });

    public static CommandInfo Call => new(
        "call",
        """
        call <func> [param] ...
        Calls a function and passes it a list of positional arguments.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
                throw new Throw("'call' does not take 0 arguments.\nType '/help call' to see its usage");

            var name = args[0];
            var values = args[1..].Select(a => new String(a)).ToList<Value>();

            var value = call.Get(name).Get();

            if (value is not Func func)
                throw new Throw("The variable was not a 'func'");

            return func.Invoke(values, new(), call);
        });

    public static CommandInfo Execute => new(
        "execute",
        """
        execute <code>
        Executes a piece of code.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length != 1)
                throw new Throw($"'execute' does not take {args.Length} arguments.\nType '/help execute' to see its usage");

            var code = args[0];

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
        });

    public static CommandInfo Load => new(
        "load",
        """
        load <path>
        Loads and execute the code stored in a text file.
        """,
        delegate (string[] args, Value input, Call call)
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
        });

    public static CommandInfo Random => new(
        "random",
        """
        random
        Returns a pseudo-random number between 0 and 1.
        
        random <max>
        Returns a pseudo-random integer between 0 and max, the max value is excluded.
        
        random <min> <max>
        Returns a pseudo-random integer between min and max, the max value is excluded.
        """,
        delegate (string[] args, Value input, Call call)
        {
            new Number(rng.NextDouble());

            if (args.Length == 0)
                return new Number(rng.NextDouble());

            if (args.Length == 1)
            {
                if (!int.TryParse(args[0], out var max))
                    throw new Throw($"Cannot parse '{args[0]}' has number");

                return new Number(rng.Next(max));
            }

            if (args.Length == 2)
            {
                if (!int.TryParse(args[0], out var min))
                    throw new Throw($"Cannot parse '{args[0]}' has number");

                if (!int.TryParse(args[1], out var max))
                    throw new Throw($"Cannot parse '{args[1]}' has number");

                return new Number(rng.Next(min, max));
            }

            throw new Throw($"'random' does not take {args.Length} arguments.\nType '/help random' to see its usage");
        });

    public static CommandInfo Time => new(
        "time",
        """
        time
        Returns the current time.
        """,
        delegate (string[] args, Value input, Call call)
        {
            if (args.Length == 0)
                return new String(DateTime.UtcNow.ToString());

            throw new Throw("'time' does not take arguments.\nType '/help time' to see its usage");
        });

    public static Engine.Builder AddDefaultCommands(this Engine.Builder engineBuilder)
    {
        typeof(DefaultCommands)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .ToList()
            .ForEach(x => engineBuilder.AddCommand((CommandInfo)x.GetValue(null)));

        return engineBuilder;
    }
}