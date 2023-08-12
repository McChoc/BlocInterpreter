using System.Collections.Generic;
using System.Linq;
using Bloc.Identifiers;
using Bloc.Tokens;
using Bloc.Utils.Constants;
using Bloc.Utils.Exceptions;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers;

internal static class IdentifierParser
{
    internal static IIdentifier Parse(List<Token> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing identifier");

        var parts = tokens.Split(x => x is SymbolToken(Symbol.COMMA));

        if (parts.Count == 1)
        {
            if (tokens.Count > 1)
                throw new SyntaxError(tokens[1].Start, tokens[1].End, "Unexpected token");

            return tokens[0] switch
            {
                INamedIdentifierToken token => token.GetIdentifier(),
                ParenthesesToken { Tokens.Count: 0 } => new TupleIdentifier(new()),
                ParenthesesToken token => Parse(token.Tokens),
                _ => throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected token"),
            };
        }
        else
        {
            if (parts[^1].Count == 0)
                parts.RemoveAt(parts.Count - 1);

            var identifiers = parts
                .Select(Parse)
                .ToList();

            return new TupleIdentifier(identifiers);
        }
    }
}