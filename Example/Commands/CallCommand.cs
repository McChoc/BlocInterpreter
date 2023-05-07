using System.Linq;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace ConsoleApp.Commands;

public sealed class CallCommand : ICommandInfo
{
    public string Name => "call";

    public string Description =>
        """
        call <func> [param] ...
        Calls a function and passes it a list of positional arguments.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length == 0)
            throw new Throw("'call' does not take 0 arguments.\nType '/help call' to see its usage");

        var name = args[0];
        var values = args[1..].Select(a => new String(a)).ToList<Value>();

        var value = call.Get(name).Get();

        if (value is not Func func)
            throw new Throw("The variable was not a 'func'");

        return func.Invoke(values, new(), call);
    }
}