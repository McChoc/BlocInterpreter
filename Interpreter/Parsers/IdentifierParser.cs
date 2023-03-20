using System.Collections.Generic;
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

        if (parts.Count != 1)
        {
            var identifiers = new List<IIdentifier>();

            foreach (var part in parts)
                identifiers.Add(Parse(part));

            return new TupleIdentifier(identifiers);
        }

        if (tokens.Count > 1)
            throw new SyntaxError(tokens[1].Start, tokens[1].End, "Unexpected token");

        switch (tokens[0])
        {
            case IIdentifierToken token:
                return new NameIdentifier(token.Text);

            case GroupToken token:
                var identifier = Parse(token.Tokens);

                if (identifier is not TupleIdentifier)
                    throw new SyntaxError(tokens[0].Start, tokens[0].End, "Tuples must contain at least 2 identifiers");

                return identifier;

            default:
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected token");
        }
    }
}