using System.Collections.Generic;
using Bloc.Tokens;

namespace Bloc.Scanners;

internal interface ITokenProvider
{
    void Skip(int count = 1);

    bool HasNext();

    Token Next();

    Token Peek();

    List<Token> PeekRange(int count);
}