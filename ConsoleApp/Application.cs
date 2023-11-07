using System;
using System.Collections.Generic;
using Bloc.Core;
using Bloc.Utils.Exceptions;
using Bloc.Values.Core;
using ConsoleApp.Utils;
using ConsoleColor = ConsoleApp.Utils.ConsoleColor;

namespace ConsoleApp;

public sealed class Application
{
    private bool _running;

    private readonly Engine _engine;
    private readonly Module _module;

    public Application(Engine engine)
    {
        _engine = engine;
        _module = new Module(AppDomain.CurrentDomain.BaseDirectory, engine);
    }

    public void Start()
    {
        _running = true;
        MainLoop();
    }

    public void Stop()
    {
        _running = false;
    }

    private void MainLoop()
    {
        while (_running)
        {
            var cancel = false;
            var depth = 0;
            var lines = new List<string>();

            while (true)
            {
                Console.Write(lines.Count == 0 ? "> " : ". ");

                var line = Console.ReadLine();

                if (line.Length > 0 && line[^1] == '\x4')
                {
                    cancel = true;
                    break;
                }

                lines.Add(line);

                if (line.Length > 0 && line[^1] == '{')
                    depth++;

                if (line.Length > 0 && line[^1] == '}')
                    depth--;

                if (depth <= 0)
                    break;
            }

            if (!cancel)
            {
                var code = string.Join("\n", lines);

                if (code.Length > 0)
                {
                    if (code[^1] != ';')
                        code += ';';

                    try
                    {
                        Engine.Compile(code, out var expression, out var statements);

                        Value value = null;

                        var exception = expression is not null
                            ? _engine.Evaluate(expression, _module, out value)
                            : _engine.Execute(statements, _module);

                        if (exception is not null)
                        {
                            ConsoleTextStyler.SetTextColor(ConsoleColor.Orange);
                            Console.WriteLine($"An exception was thrown : {exception.Value}");
                            Console.ResetColor();
                        }
                        else if (value is not null)
                        {
                            ConsoleTextStyler.SetTextColor(ConsoleColor.Green);
                            Console.WriteLine(value.ToString());
                            Console.ResetColor();
                        }
                    }
                    catch (SyntaxError e)
                    {
                        ConsoleTextStyler.SetTextColor(ConsoleColor.Red);
                        Console.WriteLine($"Syntax error : {e.Message}");
                        Console.ResetColor();
                    }
                }
            }

            Console.WriteLine();
        }
    }
}