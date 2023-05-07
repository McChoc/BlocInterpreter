using System.Collections.Generic;
using Bloc.Core;
using Bloc.Results;
using Bloc.Utils.Exceptions;
using Bloc.Values;
using ConsoleApp.Utils;
using static System.Console;

namespace ConsoleApp;

public sealed class Console
{
    private const byte RED = 9;
    private const byte ORANGE = 208;

    private bool _running;

    private readonly Engine _engine;

    public Console(Engine engine)
    {
        _engine = engine;
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
                Write(lines.Count == 0 ? "> " : ". ");

                var line = ReadLine();

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

                        IResult result;
                        Value value = null;

                        if (expression is not null)
                            result = _engine.Evaluate(expression, out value);
                        else
                            result = _engine.Execute(statements);

                        if (result is Throw t)
                        {
                            ConsoleTextStyler.SetTextColor(ORANGE);
                            WriteLine($"An exception was thrown : {t.Value}");
                            ResetColor();
                        }
                        else if (value is not null)
                        {
                            WriteLine(value.ToString());
                        }
                    }
                    catch (SyntaxError e)
                    {
                        ConsoleTextStyler.SetTextColor(RED);
                        WriteLine($"Syntax error : {e.Message}");
                        ResetColor();
                    }
                }
            }

            WriteLine();
        }
    }
}