using System;
using System.Collections.Generic;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using String = Bloc.Values.String;

namespace Bloc.Statements;

internal sealed class CommandStatement : Statement
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
            call.Engine.Log(@string.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Command);
    }

    public override bool Equals(object other)
    {
        return other is CommandStatement statement &&
            Label == statement.Label &&
            Command.Equals(statement.Command);
    }
}