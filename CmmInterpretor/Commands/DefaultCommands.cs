using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Linq;
using System.Reflection;

namespace CmmInterpretor.Commands
{
    public static class DefaultCommands
    {
        private static readonly System.Random rng = new();

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

                    if (!input.Implicit(out String str))
                        return new String("The input could not be converted to a string.");

                    if (!call.Engine.Commands.TryGetValue(str.Value, out Command command))
                        return new String("Unknown command.");

                    return new String(command.Description);
                }
                
                if (args.Length == 1)
                {
                    if (!call.Engine.Commands.TryGetValue(args[0], out Command command))
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
                        return new String($"The input was empty.");

                    if (!input.Implicit(out String str))
                        return new String("The input could not be converted to a string.");

                    return str;
                }
                
                if (args.Length == 1)
                    return new String(args[0]);

                return new String($"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage.");
            }
        );

        public static Command Clear => new(
            "clear",

            "clear\n" +
            "Clears the console.",

            (args, _, call) =>
            {
                if (args.Length != 0)
                    return new String($"'clear' does not take arguments.\nType '/help clear' to see its usage.");

                call.Engine.Clear();
                return new Void();
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
                        return new String($"The input was empty.");

                    if (!input.Implicit(out String str))
                        return new String("The input could not be converted to a string.");

                    if (!call.TryGet(str.Value, out var ptr))
                        return new String("The variable was not defined.");

                    return ptr.Get();
                }

                if (args.Length == 1)
                {
                    if (!call.TryGet(args[0], out var ptr))
                        return new String("The variable was not defined.");

                    return ptr.Get();
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
                    return new String($"'set' does not take 0 arguments.\nType '/help set' to see its usage.");

                if (args.Length == 1)
                {
                    if (input is Void)
                        return new String($"The input was empty.");

                    var name = args[0];
                    var value = input;

                    call.Set(name, new Variable(name, value, call.Scopes[^1]));

                    return new Void();
                }

                if (args.Length == 2)
                {
                    var name = args[0];
                    var value = new String(args[1]);

                    call.Set(name, new Variable(name, value, call.Scopes[^1]));

                    return new Void();
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
                    return new String($"'set' does not take 0 arguments.\nType '/help set' to see its usage.");

                var name = args[0];
                var variables = new Array(args[1..].Select(a => new String(a)).ToList<Value>());

                if (!call.TryGet(name, out var ptr))
                    return new String("The variable was not defined.");

                if (!ptr.Get().Implicit(out Function func))
                    return new String("The variable could not be converted to a function.");

                var result = func.Call(variables, call.Engine);

                if (result is IValue value)
                    return value.Value();

                if (result is Throw t)
                    return t.value;

                return new String("Cannot exit the program from a command.");
            }
        );

        //public static Command Execute => new Command(
        //    "execute",

        //    "execute <code> [argument] ...\n" +
        //    "Executes a piece of code and passes it a list of arguments.",

        //    (args, pipe, call) => new Null()
        //);

        //public static Command Random => new Command(
        //    "random",

        //    "random\n" +
        //    "Returns a pseudo-random number between 0 and 1.\n" +
        //    "\n" +
        //    "random <max>\n" +
        //    "Returns a pseudo-random integer between 0 and max, the max value is excluded.\n" +
        //    "\n" +
        //    "random <min> <max>\n" +
        //    "Returns a pseudo-random integer between min and max, the max value is excluded.",

        //    (args, pipe, _) => new Number(rng.NextDouble())
        //);

        //public static Command Time => new Command(
        //    "time",

        //    "time\n" +
        //    "Returns the current time.",

        //    (args, pipe, _) => new String(System.DateTime.Now.ToString())
        //);

        public static Engine.Builder AddDefaultCommands (this Engine.Builder engineBuilder)
        {
            typeof(DefaultCommands).GetProperties(BindingFlags.Public | BindingFlags.Static).ToList().ForEach(p => engineBuilder.AddCommand((Command)p.GetValue(null)));
            return engineBuilder;
        }
    }
}
