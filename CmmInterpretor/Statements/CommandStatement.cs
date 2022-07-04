using System;
using System.Collections.Generic;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using String = CmmInterpretor.Values.String;
using Void = CmmInterpretor.Values.Void;

namespace CmmInterpretor.Statements
{
    internal class CommandStatement : Statement
    {
        internal List<List<Token>> Commands { get; } = new();

        internal override Result? Execute(Call call)
        {
            Value output = Void.Value;

            foreach (var command in Commands)
            {
                var words = new List<string>();

                foreach (var token in command)
                {
                    try
                    {
                        words.AddRange(GetText(token, call));
                    }
                    catch (Result result)
                    {
                        return result;
                    }
                }

                var name = words[0];
                var args = words.ToArray()[1..];

                if (call.Engine.Commands.TryGetValue(name, out var c))
                    output = c.Call(args, output, call);
                else
                    output = new String("Unknown command.");
            }

            if (output.Is(out String? str))
                call.Engine.Log(str!.Value);

            return null;
        }

        private static string[] GetText(Token token, Call call)
        {
            switch (token)
            {
                case Literal literal:
                    return new[] { ((String)literal.Expression.Evaluate(call)).Value };

                case { Type: TokenType.Keyword }:
                    return new[] { token.Text };

                case { Type: TokenType.Identifier, Text: string identifier }:
                    if (!call.TryGet(identifier, out var variable))
                        throw new Throw($"Variable '{identifier}' was not defined in scope");

                    if (!variable!.Value.Is(out String? str))
                        throw new Throw("Cannot implicitly cast to string");

                    return str!.Value.Split(' ');

                default:
                    throw new Exception();
            }
        }
    }
}