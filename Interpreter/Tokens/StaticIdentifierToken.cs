using Bloc.Identifiers;

namespace Bloc.Tokens;

internal sealed class StaticIdentifierToken : TextToken, IStaticIdentifierToken
{
    internal StaticIdentifierToken(int start, int end, string text)
        : base(start, end, text) { }

    public INamedIdentifier GetIdentifier()
    {
        return new StaticIdentifier(Text);
    }
}