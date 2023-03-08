using System.Collections.Generic;
using Bloc.Constants;
using Bloc.Exceptions;
using Bloc.Identifiers;
using Bloc.Tokens;
using Bloc.Utils.Extensions;

namespace Bloc.Parsers;

internal static class IdentifierParser
{
    internal static IIdentifier Parse(List<Token> tokens)
    {
        if (tokens.Count == 0)
            throw new SyntaxError(0, 0, "Missing identifier");

        if (tokens.Count > 1)
            throw new SyntaxError(tokens[1].Start, tokens[1].End, "Unexpected token");

        switch (tokens[0])
        {
            case IIdentifierToken token:
                return new NameIdentifier(token.Text);

            case GroupToken token:
                var parts = token.Tokens.Split(x => x is SymbolToken(Symbol.COMMA));

                if (parts.Count == 1)
                    throw new SyntaxError(token.Start, token.End, "Tuples must contain at least 2 identifiers");

                var identifiers = new List<IIdentifier>();

                foreach (var part in parts)
                    identifiers.Add(Parse(part));

                return new TupleIdentifier(identifiers);

            default:
                throw new SyntaxError(tokens[0].Start, tokens[0].End, "Unexpected token");
        }
    }
}