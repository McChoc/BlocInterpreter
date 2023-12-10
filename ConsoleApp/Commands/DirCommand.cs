using System.Linq;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class DirCommand : ICommandInfo
{
    public string Name => "dir";

    public string Description =>
        """
        dir
        Returns a list of all the variables in scope.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length != 0)
            throw new Throw($"'dir' does not take {args.Length} arguments.\nType '/help dir' to see its usage");

        var variables = call.Module.TopLevelScope.Variables
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value.AsEnumerable().Select(y => $"{x.Key}\t=\t{y.Value}"));

        var text = string.Join('\n', variables);

        return new String(text);
    }
}