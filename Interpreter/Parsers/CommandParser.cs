using System.Collections.Generic;
using System.Linq;
using Bloc.Commands;
using Bloc.Commands.Arguments;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers;

internal static class CommandParser
{
    internal static Command Parse(List<IToken> tokens)
    {
        var calls = tokens
            .Split(x => x is SymbolToken(Symbol.PIPE))
            .Select(ParseCall)
            .ToList();

        return new Command(calls);
    }

    private static CommandCall ParseCall(List<IToken> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Empty command");

        var arguments = new List<IArgument>();

        for (int i = 0; i < tokens.Count; i++)
        {
            if (i < tokens.Count - 1 && tokens[i] is SymbolToken(Symbol.STAR))
                arguments.Add(ParseArgument(tokens[++i], true));
            else
                arguments.Add(ParseArgument(tokens[i]));
        }

        return new CommandCall(arguments);
    }

    private static IArgument ParseArgument(IToken token, bool unpack = false)
    {
        if (token is SymbolToken)
            throw new SyntaxError(token.Start, token.End, "Unexpected token");

        if (token is IKeywordToken keyword)
            return new LiteralArgument(keyword.Text);

        var expression = ExpressionParser.Parse(new() { token });

        return new ExpressionArgument(expression, unpack);
    }
}