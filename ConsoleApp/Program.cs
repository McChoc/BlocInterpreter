using System;
using Bloc.Core;
using ConsoleApp.Utils;

namespace ConsoleApp;

public static class Program
{
    public static void Main()
    {
        var engine = new EngineBuilder()
            .UseOutput(Console.WriteLine)
            .AddAlias("", "/Users/Alexandre/Desktop/")
            .AddAlias("std", "/Users/Alexandre/Desktop/std/")
            .AddHelpCommand()
            .AddAllCommands()
            .Build();

        var app = new Application(engine);
        engine.State = app;
        app.Start();
    }
}