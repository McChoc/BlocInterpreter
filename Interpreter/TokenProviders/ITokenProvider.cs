using System.Collections.Generic;
using Bloc.Tokens;

namespace Bloc.Scanners;

internal interface ITokenProvider
{
    void Skip(int count = 1);

    bool HasNext();

    IToken Next();

    IToken Peek();

    List<IToken> PeekRange(int count);
}