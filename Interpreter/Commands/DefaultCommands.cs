using System;
using System.Linq;
using System.Reflection;
using Bloc.Results;
using Bloc.Values;
using String = Bloc.Values.String;
using Void = Bloc.Values.Void;

namespace Bloc.Commands
{
    public static class DefaultCommands
    {
        private static readonly Random rng = new();

        public static Command Help => new(
            "help",
            "help\n" +
            "Returns a list of all commands.\n" +
            "\n" +
            "help <command>\n" +
            "<command: string> |> help\n" +
            "Returns the description of the given command.",
            (args, input, call) =>
            {
                if (args.Length == 0)
                {
                    if (input is Void)
                    {
                        return new String("For more informations on a specific command, type '/help <command>'.\n\n" +
                            string.Join("\n", call.Engine.Commands.Select(p => p.Value.Name)));
                    }

                    if (input.Is(out String? str))
                    {
                        if (!call.Engine.Commands.TryGetValue(str!.Value, out var command))
                            throw new Throw("Unknown command.");

                        return new String(command.Description);
                    }
                        
                    throw new Throw("The input could not be converted to a string.");
                }

                if (args.Length == 1)
                {
                    if (!call.Engine.Commands.TryGetValue(args[0], out var command))
                        throw new Throw("Unknown command.");

                    return new String(command.Description);
                }

                throw new Throw($"'help' does not take {args.Length} arguments.\nType '/help help' to see its usage.");
            }
        );

        public static Command Echo => new(
            "echo",
            "echo <message>\n" +
            "<message: string> |> echo\n" +
            "Returns the message.",
            (args, input, _) =>
            {
                if (args.Length == 0)
                {
                    if (!input.Is(out String? str))
                        throw new Throw("The input could not be converted to a string.");

                    return str!;
                }

                if (args.Length == 1)
                    return new String(args[0]);

                throw new Throw($"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage.");
            }
        );

        public static Command Clear => new(
            "clear",
            "clear\n" +
            "Clears the console.",
            (args, _, call) =>
            {
                if (args.Length != 0)
                    throw new Throw("'clear' does not take arguments.\nType '/help clear' to see its usage.");

                call.Engine.Clear();
                return Void.Value;
            }
        );

        public static Command Get => new(
            "get",
            "get <name>\n" +
            "<name: string> |> get\n" +
            "Gets the value of the variable with the specified name.",
            (args, input, call) =>
            {
                if (args.Length == 0)
                {
                    if (!input.Is(out String? str))
                        throw new Throw("The input could not be converted to a string.");

                    return call.Get(str!.Value).Get();
                }

                if (args.Length == 1)
                {
                    return call.Get(args[0]).Get();
                }

                throw new Throw($"'get' does not take {args.Length} arguments.\nType '/help get' to see its usage.");
            }
        );

        public static Command Set => new(
            "set",
            "set <name> <value>\n" +
            "<value> |> set <name>\n" +
            "Sets a value to a variable with a specified name. It doesn't matter if the variable was previously defined or not. The variable will always be set inside the current scope.",
            (args, input, call) =>
            {
                if (args.Length == 0)
                    throw new Throw("'set' does not take 0 arguments.\nType '/help set' to see its usage.");

                if (args.Length == 1)
                {
                    if (input is Void)
                        throw new Throw("The input was empty.");

                    var name = args[0];
                    var value = input;

                    call.Set(name, value);

                    return Void.Value;
                }

                if (args.Length == 2)
                {
                    var name = args[0];
                    var value = new String(args[1]);

                    call.Set(name, value);

                    return Void.Value;
                }

                throw new Throw($"'set' does not take {args.Length} arguments.\nType '/help set' to see its usage.");
            }
        );

        public static Command Call => new(
            "call",
            "call <function> [param] ...\n" +
            "Calls a function and passes it a list of arguments. Arguments can be accessed by the params keyword.",
            (args, _, call) =>
            {
                if (args.Length == 0)
                    throw new Throw("'call' does not take 0 arguments.\nType '/help call' to see its usage.");

                var name = args[0];
                var values = args[1..].Select(a => new String(a)).ToList<Value>();

                var value = call.Get(name).Get();

                if (!value.Is(out Function? func))
                    throw new Throw("The variable could not be converted to a function.");

                return func!.Invoke(values, call);
            }
        );

        //public static Command Execute => new Command(
        //    "execute",

        //    "execute <code> [argument] ...\n" +
        //    "Executes a piece of code and passes it a list of arguments.",

        //    (args, pipe, call) => new Null()
        //);

        public static Command Random => new(
            "random",
            "random\n" +
            "Returns a pseudo-random number between 0 and 1.\n" +
            "\n" +
            "random <max>\n" +
            "Returns a pseudo-random integer between 0 and max, the max value is excluded.\n" +
            "\n" +
            "random <min> <max>\n" +
            "Returns a pseudo-random integer between min and max, the max value is excluded.",
            (args, pipe, _) =>
            {
                new Number(rng.NextDouble());

                if (args.Length == 0)
                    return new Number(rng.NextDouble());

                if (args.Length == 1)
                {
                    if (!int.TryParse(args[0], out var max))
                        throw new Throw($"Cannot parse '{args[0]}' has number.");

                    return new Number(rng.Next(max));
                }

                if (args.Length == 2)
                {
                    if (!int.TryParse(args[0], out var min))
                        throw new Throw($"Cannot parse '{args[0]}' has number.");

                    if (!int.TryParse(args[1], out var max))
                        throw new Throw($"Cannot parse '{args[1]}' has number.");

                    return new Number(rng.Next(min, max));
                }

                throw new Throw($"'random' does not take {args.Length} arguments.\nType '/help random' to see its usage.");
            }
        );

        public static Command Time => new(
            "time",
            "time\n" +
            "Returns the current time.",
            (args, pipe, _) =>
            {
                if (args.Length == 0)
                    return new String(DateTime.UtcNow.ToString());

                throw new Throw("'time' does not take arguments.\nType '/help time' to see its usage.");
            }
        );

        public static Engine.Builder AddDefaultCommands(this Engine.Builder engineBuilder)
        {
            typeof(DefaultCommands).GetProperties(BindingFlags.Public | BindingFlags.Static).ToList()
                .ForEach(p => engineBuilder.AddCommand((Command)p.GetValue(null)));
            return engineBuilder;
        }
    }
}