using Bloc.Core;
using ConsoleApp.Utils;

namespace ConsoleApp;

public static class Program
{
    public static void Main()
    {
        var engine = new EngineBuilder()
            .UseOutput(System.Console.WriteLine)
            .AddHelpCommand()
            .AddAllCommands()
            .Build();

        var console = new Console(engine);
        engine.State = console;
        console.Start();
    }
}