using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class DeleteGlobalCommand : ICommandInfo
{
    public string Name => "delete_global";

    public string Description =>
        """
        delete_global
        Deletes all global variables
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length != 0)
            throw new Throw("'delete_global' does not take arguments.\nType '/help delete_global' to see its usage.");

        foreach (var stack in call.Module.TopLevelScope.Variables.Values)
            while (stack.Count > 0)
                stack.Peek().Delete();

        return Void.Value;
    }
}
