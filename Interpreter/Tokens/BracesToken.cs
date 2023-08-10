using System.Collections.Generic;
using Bloc.Utils.Attributes;

namespace Bloc.Tokens;

[Record]
internal sealed partial class BracesToken : GroupToken
{
    internal BracesToken(int start, int end, List<Token> tokens)
        : base(start, end, tokens) { }
}