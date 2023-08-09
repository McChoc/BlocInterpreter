using System;
using System.Linq;
using System.Reflection;
using Bloc.Commands;
using Bloc.Core;

namespace ConsoleApp.Utils;

public static class EngineBuilderExtensions
{
    public static EngineBuilder AddAllCommands(this EngineBuilder engineBuilder)
    {
        var commandTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(ICommandInfo)));

        foreach (var commandType in commandTypes)
            engineBuilder.AddCommand(commandType);

        return engineBuilder;
    }
}