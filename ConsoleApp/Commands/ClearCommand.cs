using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class ClearCommand : ICommandInfo
{
    public string Name => "clear";

    public string Description =>
        """
        clear
        Clears the console.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length != 0)
            throw new Throw("'clear' does not take arguments.\nType '/help clear' to see its usage");

        System.Console.Clear();
        return Void.Value;
    }
}
