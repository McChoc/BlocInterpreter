using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Void = Bloc.Values.Void;

namespace ConsoleApp.Commands;

public class DeleteGlobalCommand : ICommandInfo
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

        foreach (var stack in call.Engine.GlobalScope.Variables.Values)
            while (stack.Count > 0)
                stack.Peek().Delete();

        return Void.Value;
    }
}
