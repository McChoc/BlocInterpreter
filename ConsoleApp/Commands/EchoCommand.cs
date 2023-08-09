using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class EchoCommand : ICommandInfo
{
    public string Name => "echo";

    public string Description =>
        """
        echo <message>
        <message: string> |> echo
        Returns the message.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length == 0)
            return input as String ?? throw new Throw("The input was not a string");

        if (args.Length == 1)
            return new String(args[0]);

        throw new Throw($"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage");
    }
}
