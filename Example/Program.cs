using CmmInterpretor;
using CmmInterpretor.Commands;
using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;
using System.Linq;
using Console = System.Console;

const byte RED = 9;
const byte ORANGE = 208;

List<string> lines = new();

Engine engine = new Engine.Builder(args)
    .OnLog(msg => Console.WriteLine(msg))
    .OnClear(() => Console.Clear())
    .AddDefaultCommands()
    .Build();

while (true)
{
    int a = 0;

    try
    {
        Console.Write("> ");
        string line = Console.ReadLine();

        bool doExecute = true;

        if (line.LastOrDefault() == '\\')
        {
            line = line[0..^1];
            doExecute = false;
        }

        lines.Add(line);

        if (doExecute)
        {
            string code = string.Join("\n", lines);
            lines.Clear();

            if (code[^1] != ';')
                code += ';';

            var result = engine.Execute(code);

            if (result is IValue value)
            {
                if (value.Value() is not Void)
                    Console.WriteLine(value.Value().ToString());
            }
            else if (result is Return || result is Exit)
            {
                break;
            }
            else if (result is Throw t)
            {
                ConsoleColor.SetColor(ORANGE);
                Console.WriteLine($"An exception was thrown : {t.value}");
                Console.WriteLine();
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }
    catch (SyntaxError e)
    {
        ConsoleColor.SetColor(RED);
        Console.WriteLine($"Syntax error : {e.Message}");
        Console.WriteLine();
        Console.ResetColor();
    }
    //catch
    //{
    //    ConsoleColor.SetColor(RED);
    //    Console.WriteLine("An unexpected error occured");
    //    Console.WriteLine();
    //    Console.ResetColor();
    //}
}