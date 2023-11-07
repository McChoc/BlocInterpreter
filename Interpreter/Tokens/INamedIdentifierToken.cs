using Bloc.Identifiers;

namespace Bloc.Tokens;

internal interface INamedIdentifierToken : IToken
{
    INamedIdentifier GetIdentifier();
}