using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Commands;

internal sealed class Command
{
    private readonly List<CommandCall> _commandCalls;

    internal Command(List<CommandCall> commandCalls)
    {
        _commandCalls = commandCalls;
    }

    internal Value Execute(Call call)
    {
        Value value = Void.Value;

        foreach (var commandCall in _commandCalls)
            value = commandCall.Execute(value, call);

        return value;
    }
}