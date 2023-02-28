using System.Collections.Generic;
using Bloc.Tokens;

namespace Bloc.Scanners;

internal interface ITokenProvider
{
    bool HasNext();

    Token Next();

    Token Peek();

    List<Token> PeekRange(int count);

    void Skip(int count = 1);
}