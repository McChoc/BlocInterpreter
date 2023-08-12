using Bloc.Identifiers;

namespace Bloc.Tokens;

internal interface INamedIdentifierToken
{
    INamedIdentifier GetIdentifier();
}