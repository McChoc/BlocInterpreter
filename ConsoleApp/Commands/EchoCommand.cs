using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;

namespace ConsoleApp.Commands;

public sealed class EchoCommand : ICommandInfo
{
    public string Name => "echo";

    public string Description =>
        """
        echo <message>
        <message> |> echo
        Returns the message.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length == 0)
            return input;

        if (args.Length == 1)
            return args[0];

        throw new Throw($"'echo' does not take {args.Length} arguments.\nType '/help echo' to see its usage");
    }
}
