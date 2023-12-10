using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Commands;

internal sealed class HelpCommand : ICommandInfo
{
    public string Name => "help";

    public string Description =>
        """
        help
        Returns a list of all commands.
        
        help <command_name>
        <command_name: string> |> help
        Returns the description of the given command.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length == 0)
        {
            if (input is Void)
            {
                var message =
                    "For more informations on a specific command, type '/help <command>'.\n" +
                    "\n" +
                    string.Join("\n", call.Engine.Commands.Values.Select(c => c.Name.ToLower()).OrderBy(x => x));

                return new String(message);
            }

            if (input is String @string)
            {
                if (!call.Engine.Commands.TryGetValue(@string.Value.ToLower(), out var command))
                    throw new Throw("Unknown command");

                return new String(command.Description);
            }

            throw new Throw("The input value was not a string");
        }

        if (args.Length == 1)
        {
            if (args[0] is not String @string)
                throw new Throw("The command name was not a string");

            if (!call.Engine.Commands.TryGetValue(@string.Value.ToLower(), out var command))
                throw new Throw("Unknown command");

            return new String(command.Description);
        }

        throw new Throw($"'help' does not take {args.Length} arguments.\nType '/help help' to see its usage");
    }
}