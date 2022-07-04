using System;
using System.Linq;
using System.Reflection;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using String = CmmInterpretor.Values.String;
using Void = CmmInterpretor.Values.Void;

namespace CmmInterpretor.Commands
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
                        return new String(
                            "For more informations on a specific command, type '/help <command>'.\n\n" +
                            string.Join("\n", call.Engine.Commands.Select(p => p.Value.Name))
                        );

                    if (!input.Is(out String? str))
                        return new String("The input could not be converted to a string.");

                    if (!call.Engine.Commands.TryGetValue(str!.Value, out var command))
                        return new String("Unknown command.");

                    return new String(command.Description);
                }

                if (args.Length == 1)
                {
                    if (!call.Engine.Commands.TryGetValue(args[0], out var command))
                        return new String("Unknown command.");

                    return new String(command.Description);
                }

                return new String($"'help' does not take {args.Length} arguments.\nType '/help' to see its usage.");
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
                    if (input is Void)
                        return new String("The input was empty.");

                    if (!input.Is(out String? str))
                        return new String("The input could not be converted to a string.");

                    return str!;
                }

                if (args.Length == 1)
                    return new String(args[0]);

                return new String(
                    $"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage.");
            }
        );

        public static Command Clear => new(
            "clear",
            "clear\n" +
            "Clears the console.",
            (args, _, call) =>
            {
                if (args.Length != 0)
                    return new String("'clear' does not take arguments.\nType '/help clear' to see its usage.");

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
                    if (input is Void)
                        return new String("The input was empty.");

                    if (!input.Is(out String? str))
                        return new String("The input could not be converted to a string.");

                    if (!call.TryGet(str!.Value, out var var))
                        return new String("The variable was not defined.");

                    return var!.Value;
                }

                if (args.Length == 1)
                {
                    if (!call.TryGet(args[0], out var var))
                        return new String("The variable was not defined.");

                    return var!.Value;
                }

                return new String($"'get' does not take {args.Length} arguments.\nType '/help get' to see its usage.");
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
                    return new String("'set' does not take 0 arguments.\nType '/help set' to see its usage.");

                if (args.Length == 1)
                {
                    if (input is Void)
                        return new String("The input was empty.");

                    var name = args[0];
                    var value = input;

                    call.Set(name, new StackVariable(value, name, call.Scopes[^1]));

                    return Void.Value;
                }

                if (args.Length == 2)
                {
                    var name = args[0];
                    var value = new String(args[1]);

                    call.Set(name, new StackVariable(value, name, call.Scopes[^1]));

                    return Void.Value;
                }

                return new String($"'get' does not take {args.Length} arguments.\nType '/help get' to see its usage.");
            }
        );

        public static Command Call => new(
            "call",
            "call <function> [param] ...\n" +
            "Calls a function and passes it a list of arguments. Arguments can be accessed by the params keyword.",
            (args, _, call) =>
            {
                if (args.Length == 0)
                    return new String("'set' does not take 0 arguments.\nType '/help set' to see its usage.");

                var name = args[0];
                var variables = args[1..].Select(a => new String(a)).ToList<Value>();

                if (!call.TryGet(name, out var var))
                    return new String("The variable was not defined.");

                if (!var!.Value.Is(out Function? func))
                    return new String("The variable could not be converted to a function.");

                return func!.Invoke(variables, call).Value;
            }
        );

        public static Command DeleteGlobal => new(
            "delete_global",
            "delete_global\n" +
            "Deletes all global variables",
            (args, _, call) =>
            {
                if (args.Length != 0)
                    return new String("'delete_global' does not take arguments.\nType '/help clear' to see its usage.");

                foreach (var variable in call.Engine.Global.Variables.Values)
                    variable.Destroy();

                return Void.Value;
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
                        return new String($"Cannot parse '{args[0]}' has number.");

                    return new Number(rng.Next(max));
                }

                if (args.Length == 2)
                {
                    if (!int.TryParse(args[0], out var min))
                        return new String($"Cannot parse '{args[0]}' has number.");

                    if (!int.TryParse(args[1], out var max))
                        return new String($"Cannot parse '{args[1]}' has number.");

                    return new Number(rng.Next(min, max));
                }

                return new String(
                    $"'random' does not take {args.Length} arguments.\nType '/help random' to see its usage.");
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

                return new String("'time' does not take arguments.\nType '/help time' to see its usage.");
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