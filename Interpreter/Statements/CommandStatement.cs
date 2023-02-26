using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Tokens;
using Bloc.Values;
using Void = Bloc.Values.Void;
using String = Bloc.Values.String;

namespace Bloc.Statements;

internal sealed class CommandStatement : Statement
{
    internal List<List<Token>> Commands { get; } = new();

    internal override IEnumerable<Result> Execute(Call call)
    {
        Value value = Void.Value;

        try
        {
            foreach (var tokens in Commands)
            {
                var words = tokens.SelectMany(t => GetText(t, call)).ToArray();

                var name = words[0];
                var args = words[1..];

                if (!call.Engine.Commands.TryGetValue(name, out var command))
                    return new[] { new Throw("Unknown command.") };

                value = command.Call(args, value, call);
            }
        }
        catch (Throw exception)
        {
            return new[] { exception };
        }

        if (String.TryImplicitCast(value, out var @string))
            call.Engine.Log(@string.Value);

        return Enumerable.Empty<Result>();

        static string[] GetText(Token token, Call call)
        {
            switch (token)
            {
                case Literal literal:
                    return new[] { ((String)literal.Expression.Evaluate(call)).Value };

                case { Type: TokenType.Keyword }:
                    return new[] { token.Text };

                case { Type: TokenType.Identifier, Text: string identifier }:
                    var value = call.Get(identifier).Get();

                    var @string = String.ImplicitCast(value);

                    return @string.Value.Split(' ');
            }

            throw new Exception();
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Commands.Count);
    }

    public override bool Equals(object other)
    {
        if (other is not CommandStatement statement)
            return false;

        if (Label != statement.Label)
            return false;

        if (Commands.Count != statement.Commands.Count)
            return false;

        for (int i = 0; i < Commands.Count; i++)
        {
            if (Commands[i].Count != statement.Commands[i].Count)
                return false;

            for (int j = 0; j < Commands[i].Count; j++)
                if (Commands[i][j] != statement.Commands[i][j])
                    return false;
        }

        return true;
    }
}