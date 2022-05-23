using CmmInterpretor;
using CmmInterpretor.Commands;
using CmmInterpretor.Results;
using CmmInterpretor.Utils.Exceptions;
using CmmInterpretor.Values;
using System.Collections.Generic;
using Console = System.Console;

const byte RED = 9;
const byte ORANGE = 208;

Engine engine = new Engine.Builder(args)
    .OnLog(msg => Console.WriteLine(msg))
    .OnClear(() => Console.Clear())
    .AddDefaultCommands()
    .Build();


while (true)
{
    try
    {
        int depth = 0;
        var lines = new List<string>();

        while (true)
        {
            Console.Write(lines.Count == 0 ? ">>> " : "... ");
            string line = Console.ReadLine();

            lines.Add(line);

            if (line.Length >= 1 && line[^1] == '{')
                depth++;

            if (line.Length >= 1 && line[^1] == '}')
                depth--;

            if (depth <= 0)
                break;
        }

        string code = string.Join("\n", lines);

        if (code.Length > 0 && code[^1] != ';')
            code += ';';

        if (code.Length > 0)
        {
            var variant = engine.Execute(code);

            if (variant is not null)
            {
                if (variant.Is(out Value value))
                {
                    if (value is not Void)
                        Console.WriteLine(value.ToString());
                }
                else if (variant.Is(out Result result))
                {
                    if (result is Throw t)
                    {
                        ConsoleColor.SetColor(ORANGE);
                        Console.WriteLine($"An exception was thrown : {t.value}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
    catch (SyntaxError e)
    {
        ConsoleColor.SetColor(RED);
        Console.WriteLine($"Syntax error : {e.Message}");
        Console.ResetColor();
    }

    Console.WriteLine();
}