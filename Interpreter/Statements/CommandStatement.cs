using System.Collections.Generic;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Types;

namespace Bloc.Statements;

[Record]
internal sealed partial class CommandStatement : Statement
{
    internal Command Command { get; }

    internal CommandStatement(Command command)
    {
        Command = command;
    }

    internal override IEnumerable<IResult> Execute(Call call)
    {
        if (!ExecuteCommand(Command, call, out var value, out var exception))
            yield return exception!;

        if (String.TryImplicitCast(value!, out var @string))
            call.Engine.Output(@string.Value);
    }
}