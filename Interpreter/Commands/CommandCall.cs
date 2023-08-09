using System.Collections.Generic;
using System.Linq;
using Bloc.Commands.Arguments;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;

namespace Bloc.Commands;

internal sealed class CommandCall
{
    private readonly List<IArgument> _arguments;

    internal CommandCall(List<IArgument> arguments)
    {
        _arguments = arguments;
    }

    internal Value Execute(Value input, Call call)
    {
        var args = _arguments
            .SelectMany(x => x.GetArguments(call))
            .ToArray();

        if (!call.Engine.Commands.TryGetValue(args[0].ToLower(), out var command))
            throw new Throw("Unknown command.");

        return command.Call(args[1..], input, call);
    }
}