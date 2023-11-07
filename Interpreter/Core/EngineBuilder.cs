using System;
using System.Collections.Generic;
using Bloc.Commands;

namespace Bloc.Core;

public sealed class EngineBuilder
{
    private Action<string> _output = _ => { };
    private readonly EngineOptions _options = new();
    private readonly Dictionary<string, string> _aliases = new();
    private readonly Dictionary<string, ICommandInfo> _commands = new();

    public Engine Build()
    {
        return new Engine(_options, _output, _aliases, _commands);
    }

    public EngineBuilder UseOptions(Action<EngineOptions> callback)
    {
        callback(_options);
        return this;
    }

    public EngineBuilder UseOutput(Action<string> callback)
    {
        _output = callback;
        return this;
    }

    public EngineBuilder AddAlias(string alias, string path)
    {
        _aliases[alias] = path;
        return this;
    }

    public EngineBuilder AddHelpCommand() => AddCommand<HelpCommand>();

    public EngineBuilder AddCommand<T>() where T : ICommandInfo, new()
    {
        var command = new T();
        _commands.Add(command.Name.ToLower(), command);
        return this;
    }

    public EngineBuilder AddCommand(Type type)
    {
        var command = (ICommandInfo)Activator.CreateInstance(type);
        _commands.Add(command.Name.ToLower(), command);
        return this;
    }
}