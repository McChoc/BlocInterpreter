﻿using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Commands;

public delegate Value CommandCallback(string[] args, Value input, Call call);

public sealed class CommandInfo
{
    private readonly CommandCallback _callback;

    public string Name { get; }
    public string Description { get; }

    public CommandInfo(string name, string description, CommandCallback callback)
    {
        Name = name;
        Description = description;
        _callback = callback;
    }

    public Value Call(string[] args, Value input, Call call) => _callback(args, input, call);
}