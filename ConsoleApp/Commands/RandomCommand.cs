using System;
using Bloc.Commands;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace ConsoleApp.Commands;

public sealed class RandomCommand : ICommandInfo
{
    private static readonly Random random = new();

    public string Name => "random";

    public string Description =>
        """
        random
        Returns a pseudo-random number between 0 and 1.
        
        random <max>
        Returns a pseudo-random integer between 0 and max, the max value is excluded.
        
        random <min> <max>
        Returns a pseudo-random integer between min and max, the max value is excluded.
        """;

    public Value Call(string[] args, Value input, Call call)
    {
        if (args.Length == 0)
            return new Number(random.NextDouble());

        if (args.Length == 1)
        {
            if (!int.TryParse(args[0], out int max))
                throw new Throw($"Cannot parse '{args[0]}' has number");

            return new Number(random.Next(max));
        }

        if (args.Length == 2)
        {
            if (!int.TryParse(args[0], out int min))
                throw new Throw($"Cannot parse '{args[0]}' has number");

            if (!int.TryParse(args[1], out int max))
                throw new Throw($"Cannot parse '{args[1]}' has number");

            return new Number(random.Next(min, max));
        }

        throw new Throw($"'random' does not take {args.Length} arguments.\nType '/help random' to see its usage");
    }
}