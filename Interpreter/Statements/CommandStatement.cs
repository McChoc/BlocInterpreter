using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Tokens;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed record CommandStatement : Statement
    {
        internal List<List<Token>> Commands { get; } = new();

        internal override Result? Execute(Call call)
        {
            Value value = Void.Value;

            foreach (var command in Commands)
            {
                try
                {
                    var words = command.SelectMany(t => GetText(t, call)).ToArray();

                    var name = words[0];
                    var args = words[1..];

                    if (!call.Engine.Commands.TryGetValue(name, out var com))
                        throw new Throw("Unknown command.");

                    value = com.Call(args, value, call);
                }
                catch (Result result)
                {
                    return result;
                }
            }

            if (String.TryImplicitCast(value, out var @string))
                call.Engine.Log(@string.Value);

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
                    var value = call.Get(identifier).Get();

                    var @string = String.ImplicitCast(value);

                    return @string.Value.Split(' ');

                default:
                    throw new System.Exception();
            }
        }
    }
}