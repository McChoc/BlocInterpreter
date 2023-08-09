using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class ExitCommand : ICommandInfo
{
    public string Name => "exit";

    public string Description =>
        """
        exit
        Exits the application.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length != 0)
            throw new Throw("'exit' does not take arguments.\nType '/help exit' to see its usage.");

        if (call.Engine.State is Console console)
            console.Stop();

        return Void.Value;
    }
}
