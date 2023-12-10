using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Commands;

[Record]
internal sealed partial class CommandCall
{
    private readonly List<CommandArg> _arguments;

    internal CommandCall(List<CommandArg> arguments)
    {
        _arguments = arguments;
    }

    internal Value Execute(Value input, Call call)
    {
        var args = _arguments
            .SelectMany(x => x.GetArguments(call))
            .ToArray();

        if (args.Length == 0)
            throw new Throw("Missing command.");

        if (args[0] is not String @string)
            throw new Throw("Unknown command.");

        if (!call.Engine.Commands.TryGetValue(@string.Value.ToLower(), out var command))
            throw new Throw("Unknown command.");

        return command.Call(args[1..], input, call);
    }
}