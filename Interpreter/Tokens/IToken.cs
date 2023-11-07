namespace Bloc.Tokens;

internal interface IToken
{
    int Start { get; }
    int End { get; }
}