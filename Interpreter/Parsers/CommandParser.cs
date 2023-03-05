using System.Collections.Generic;
using System.Linq;
using Bloc.Commands;
using Bloc.Commands.Arguments;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers;

internal class CommandParser
{
    internal static Command Parse(List<Token> tokens)
    {
        var calls = tokens
            .Split(x => x is SymbolToken(Symbol.PIPE))
            .Select(ParseCall)
            .ToList();

        return new Command(calls);
    }

    private static CommandCall ParseCall(List<Token> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Empty command");

        var arguments = new List<IArgument>();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (i < tokens.Count - 1 && tokens[i] is SymbolToken(Symbol.UNPACK_ARRAY))
                arguments.Add(ParseArgument(tokens[++i], true));
            else
                arguments.Add(ParseArgument(tokens[i]));
        }

        return new CommandCall(arguments);
    }

    private static IArgument ParseArgument(Token token, bool unpack = false)
    {
        return token switch
        {
            IKeywordToken word
                => new LiteralArgument(word.Text),
            LiteralToken or NumberToken or StringToken or ArrayToken or StructToken or GroupToken or IdentifierToken
                => new ExpressionArgument(ExpressionParser.Parse(new() { token }), unpack),
            _   => throw new SyntaxError(token.Start, token.End, "Unexpected token")
        };
    }
}