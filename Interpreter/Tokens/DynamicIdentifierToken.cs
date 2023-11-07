using System.Collections.Generic;
using Bloc.Identifiers;
using Bloc.Parsers;

namespace Bloc.Tokens;

internal sealed class DynamicIdentifierToken : GroupToken, INamedIdentifierToken
{
    internal DynamicIdentifierToken(int start, int end, List<IToken> tokens)
        : base(start, end, tokens) { }

    public INamedIdentifier GetIdentifier()
    {
        var expression = ExpressionParser.Parse(Tokens);

        return new DynamicIdentifier(expression);
    }
}