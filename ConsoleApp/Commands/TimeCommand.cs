using System;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using String = Bloc.Values.Types.String;

namespace ConsoleApp.Commands;

public sealed class TimeCommand : ICommandInfo
{
    public string Name => "time";

    public string Description =>
        """
        time
        Returns the current time.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length == 0)
            return new String(DateTime.UtcNow.ToString());

        throw new Throw("'time' does not take arguments.\nType '/help time' to see its usage");
    }
}