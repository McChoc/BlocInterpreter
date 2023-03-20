using System;
using System.Collections.Generic;
using Bloc;
using Bloc.Commands;
using Bloc.Results;
using Bloc.Utils.Exceptions;
using Bloc.Values;
using Void = Bloc.Values.Void;
using ConsoleColor = Example.ConsoleColor;

const byte RED = 9;
const byte ORANGE = 208;

bool running = true;

var engine = new Engine.Builder(args)
    .OnLog(Console.WriteLine)
    .OnClear(Console.Clear)
    .OnExit(() => running = false)
    .AddDefaultCommands()
    .AddCommand(new(
        "delete_global",
        "delete_global\n" +
        "Deletes all global variables",
        (args, _, call) =>
        {
            if (args.Length != 0)
                throw new Throw("'delete_global' does not take arguments.\nType '/help delete_global' to see its usage.");

            foreach (var stack in call.Engine.GlobalScope.Variables.Values)
                while (stack.Count > 0)
                    stack.Peek().Delete();

            return Void.Value;
        }))
    .Build();

while (running)
{
    var cancel = false;
    var depth = 0;
    var lines = new List<string>();

    while (true)
    {
        Console.Write(lines.Count == 0 ? ">>> " : "... ");

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

                IResult result;
                Value value = null;

                if (expression is not null)
                    result = engine.Evaluate(expression, out value);
                else
                    result = engine.Execute(statements);

                if (result is Throw t)
                {
                    ConsoleColor.SetColor(ORANGE);
                    Console.WriteLine($"An exception was thrown : {t.Value}");
                    Console.ResetColor();
                }
                else if (value is not null)
                {
                    Console.WriteLine(value.ToString());
                }
            }
            catch (SyntaxError e)
            {
                ConsoleColor.SetColor(RED);
                Console.WriteLine($"Syntax error : {e.Message}");
                Console.ResetColor();
            }
        }
    }

    Console.WriteLine();
}