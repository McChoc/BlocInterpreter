using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class GetCommand : ICommandInfo
{
    public string Name => "get";

    public string Description =>
        """
        get <name>
        <name: string> |> get
        Gets the value of the variable with the specified name.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length == 0)
        {
            if (input is not String @string)
                throw new Throw("The input was not a string");

            return call.Get(@string.Value).Get();
        }

        if (args.Length == 1)
        {
            if (args[0] is not String @string)
                throw new Throw("The name was not a string");

            return call.Get(@string.Value).Get();
        }

        throw new Throw($"'get' does not take {args.Length} arguments.\nType '/help get' to see its usage");
    }
}