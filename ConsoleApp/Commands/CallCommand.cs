using System.Linq;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class CallCommand : ICommandInfo
{
    public string Name => "call";

    public string Description =>
        """
        call <func> [param] ...
        Calls a function and passes it a list of positional arguments.
        """;

    public Value Call(Value[] args, Value input, Call call)
    {
        if (args.Length == 0)
            throw new Throw("'call' does not take 0 arguments.\nType '/help call' to see its usage");

        if (args[0] is not String @string)
            throw new Throw("The function name was not a string");

        var value = call.Get(@string.Value).Get();

        if (value is not Func func)
            throw new Throw("The variable was not a 'func'");

        var funcArguments = args[1..].ToList();
        return func.Invoke(funcArguments, new(), call);
    }
}