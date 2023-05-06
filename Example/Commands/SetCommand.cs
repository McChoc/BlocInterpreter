using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace ConsoleApp.Commands;

public class SetCommand : ICommandInfo
{
    public string Name => "set";

    public string Description =>
        """
        set <name> <value>
        <value> |> set <name>
        Creates a new variable with a specified name and value in the current scope.
        """;

    public Value Call(string[] args, Value input, Call call)
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
    }
}