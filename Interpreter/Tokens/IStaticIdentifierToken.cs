namespace Bloc.Tokens;

internal interface IStaticIdentifierToken : INamedIdentifierToken
{
    string Text { get; }
}